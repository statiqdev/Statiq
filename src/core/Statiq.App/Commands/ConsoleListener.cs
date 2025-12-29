using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.App
{
    public class ConsoleListener
    {
        private readonly Func<Task> _onCancel;
        private readonly Func<string, Task> _onReadLine;
        private readonly InterlockedBool _readingLines = new InterlockedBool();
        private readonly List<string> _history = new List<string>();
        private readonly object _bufferLock = new object();
        private StringBuilder _buffer = new StringBuilder();
        private int _index;
        private int _startingLeft;
        private int _startingTop;
        private int _historyIndex;

        // Note that the callbacks will get invoked on a different thread
        public ConsoleListener(Func<Task> onCancel, Func<string, Task> onReadLine = null, bool startReadingLines = false)
        {
            _onCancel = onCancel;
            _onReadLine = onReadLine;

            // Only listen if console input has not been redirected, otherwise it's on the caller
#pragma warning disable VSTHRD110 // Observe the awaitable result of this method call by awaiting it, assigning to a variable, or passing it to another method.
            if (!Console.IsInputRedirected)
            {
                Console.TreatControlCAsInput = true;
                Task.Run(ReadAsync);
            }
#pragma warning restore VSTHRD110

            if (startReadingLines)
            {
                StartReadingLines();
            }
        }

        // Inspired by https://stackoverflow.com/a/49511467
        private async Task ReadAsync()
        {
            while (true)
            {
                if (!Console.KeyAvailable)
                {
                    await Task.Delay(50);
                    continue;
                }

                // Console.CancelKeyPress bubbles up to calling batch files so check manually instead
                // The (ConsoleKey)3 check is to support a bug in VS Code: https://github.com/Microsoft/vscode/issues/9347 (possibly others?)
                // Intercept the key press if we're not reading lines (otherwise it'll be output to screen)
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                // Is it an exit key?
                if (keyInfo.Key == (ConsoleKey)3 || (keyInfo.Key == ConsoleKey.C && (keyInfo.Modifiers & ConsoleModifiers.Control) != 0))
                {
                    _readingLines.Unset();
                    Console.WriteLine();
                    try
                    {
                        await _onCancel?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }

                // Are we reading?
                string line = null;
                if (_readingLines)
                {
                    lock (_bufferLock)
                    {
                        // Does it map to a Unicode char (https://docs.microsoft.com/en-us/dotnet/api/system.consolekeyinfo.keychar#remarks)
                        if (keyInfo.KeyChar != 0 && !char.IsControl(keyInfo.KeyChar))
                        {
                            Console.Write(keyInfo.KeyChar);
                            _buffer.Insert(_index++, keyInfo.KeyChar);

                            // Write out any remaining buffer if we're inserting not at the end
                            if (_index < _buffer.Length)
                            {
                                int left = Console.CursorLeft;
                                int top = Console.CursorTop;
                                Console.Write(_buffer.ToString().Substring(_index));
                                SetCursorPosition(left, top);
                            }
                        }
                        else if (keyInfo.Key == ConsoleKey.Enter)
                        {
                            // Handle the action outside the lock or we might deadlock
                            Console.WriteLine();
                            line = _buffer.ToString();
                        }
                        else if (keyInfo.Key == ConsoleKey.Backspace)
                        {
                            if (_index > 0)
                            {
                                _index--;
                                _buffer.Remove(_index, 1);

                                (int left, int top) = MoveLeft();
                                Console.Write(_buffer.ToString().Substring(_index) + " ");
                                SetCursorPosition(left, top);
                            }
                        }
                        else if (keyInfo.Key == ConsoleKey.Delete)
                        {
                            if (_index < _buffer.Length)
                            {
                                _buffer.Remove(_index, 1);

                                int left = Console.CursorLeft;
                                int top = Console.CursorTop;
                                Console.Write(_buffer.ToString().Substring(_index) + " ");
                                SetCursorPosition(left, top);
                            }
                        }
                        else if (keyInfo.Key == ConsoleKey.LeftArrow)
                        {
                            if (_index > 0)
                            {
                                _index--;
                                MoveLeft();
                            }
                        }
                        else if (keyInfo.Key == ConsoleKey.RightArrow)
                        {
                            if (_index < _buffer.Length)
                            {
                                _index++;
                                MoveRight();
                            }
                        }
                        else if (keyInfo.Key == ConsoleKey.Home)
                        {
                            _index = 0;
                            int top = Console.CursorTop;
                            SetCursorPosition(0, top);
                        }
                        else if (keyInfo.Key == ConsoleKey.End)
                        {
                            if (_index < _buffer.Length)
                            {
                                // Writing the buffer gets us to the end
                                Console.Write(_buffer.ToString().Substring(_index));
                                _index = _buffer.Length;
                            }
                        }
                        else if (keyInfo.Key == ConsoleKey.UpArrow)
                        {
                            if (_historyIndex > 0)
                            {
                                _historyIndex--;
                                RecallHistory();
                            }
                        }
                        else if (keyInfo.Key == ConsoleKey.DownArrow)
                        {
                            if (_historyIndex + 1 < _history.Count)
                            {
                                _historyIndex++;
                                RecallHistory();
                            }
                        }
                        else if (keyInfo.Key == ConsoleKey.Escape)
                        {
                            SetCursorPosition(0, 0);
                            Console.Write(new string(' ', _buffer.Length));
                            SetCursorPosition(0, 0);
                            _buffer.Clear();
                            _index = 0;
                        }
                    }
                }

                // Did we get a line?
                if (line is object)
                {
                    // We won't have gotten a line unless we're reading, and we don't read unless there's an action
                    // so we don't need to check the action here for null before invoking it
                    try
                    {
                        await _onReadLine(line);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                    // Add to history, but only if not empty
                    if (line != string.Empty)
                    {
                        _history.Add(line);
                    }

                    // Only prompt if we didn't stop reading inside the action
                    ResetBuffer(_readingLines);
                }
            }
        }

        private (int, int) MoveLeft()
        {
            int left = Console.CursorLeft - 1;
            int top = Console.CursorTop;

            // First column, move up
            if (left < 0)
            {
                left = Console.BufferWidth - 1;
                top--;
            }

            return SetCursorPosition(left, top);
        }

        private (int, int) MoveRight()
        {
            int left = Console.CursorLeft + 1;
            int top = Console.CursorTop;

            // Last column, move down
            if (left >= Console.BufferWidth)
            {
                left = 0;
                top++;
            }

            return SetCursorPosition(left, top);
        }

        // Safely sets the position no earlier than the start
        private (int, int) SetCursorPosition(int left, int top)
        {
            if (top < _startingTop)
            {
                top = _startingTop;
            }
            if (top == _startingTop && left < _startingLeft)
            {
                left = _startingLeft;
            }
            Console.SetCursorPosition(left, top);
            return (left, top);
        }

        private void RecallHistory()
        {
            if (_historyIndex >= 0 && _historyIndex < _history.Count)
            {
                // Set the buffer to the recalled line
                int originalLength = _buffer.Length;
                _buffer.Clear();
                _buffer.Append(_history[_historyIndex]);
                _index = _history[_historyIndex].Length;

                // Write out the recalled line
                SetCursorPosition(0, 0);
                Console.Write(_history[_historyIndex]);

                // Write over any left over content from the previous line
                if (originalLength > _history[_historyIndex].Length)
                {
                    int left = Console.CursorLeft;
                    int top = Console.CursorTop;
                    Console.Write(new string(' ', originalLength - _history[_historyIndex].Length));
                    SetCursorPosition(left, top);
                }
            }
        }

        private void ResetBuffer(bool prompt)
        {
            lock (_bufferLock)
            {
                _buffer.Clear();
                _index = 0;
                _historyIndex = _history.Count;
                if (prompt)
                {
                    Console.Write("> ");
                }
                _startingLeft = Console.CursorLeft;
                _startingTop = Console.CursorTop;
            }
        }

        public void StartReadingLines()
        {
            if (!_readingLines && _onReadLine is object)
            {
                ResetBuffer(true);
                _readingLines.Set();
            }
        }

        public void StopReadingLines()
        {
            _readingLines.Unset();
            Console.WriteLine();
        }
    }
}
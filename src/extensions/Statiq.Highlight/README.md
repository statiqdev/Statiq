To update `highlight.js` download a CDN copy from one of the standard CDNs listed at https://highlightjs.org/download/.

All usages of `console` should be commented out (I.e. `console.log()`, etc.).

It then needs a few tweaks to be compliant with the .NET regex engine:

- for python replace
  ```
  const IDENT_RE = /[\p{XID_Start}_]\p{XID_Continue}*/u;
  ```
  with
  ```
  const IDENT_RE = /[a-zA-Z0-9_-]*/u;
  ```
  Note that `\p{XID_Start}` and `\p{XID_Continue}` are unicode character classes that are not supported by the .NET regex engine (at least at the time this was written). Replacing them with the equivalent character classes works well enough, but does result in a loss of parsing fidelity. More information is at https://perldoc.perl.org/perlrecharclass#Unicode-Properties.
- for ada replace 
  ```
  var BAD_CHARS = '[]{}%#\'\"'
  ```
  with
  ```
  var BAD_CHARS = '{}%#\'\"'
  ```
- for lisp replace
  ```
  var MEC_RE = '\\|[^]*?\\|';
  ```
  with
  ```
  var MEC_RE = '\\|[\S\s]*?\\|';
  ```
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class AddRtlSupportFixture : BaseFixture
    {
        [Test]
        public async Task AddProperDirectionAttributesToRtlBlocks()
        {
            // Given
            TestDocument document = new TestDocument(@"
<h1>Hello World <span>سڵاو جیهان</span> Hello World</h1>
<p>This is very nice.</p>
<div>
  <p>اهلا و سهلا</p>
  <p>This is so cool</p>
</div>

<ul>
<li>بەڵێ</li>
<li>Yes</li>
</ul>

<article>
<h2>مانگ</h2>
<p>مانگ ھەروەھا پێی دەوتریت ھەیڤ (بە ئینگلیزی: Moon، بە عەرەبی: القمر) بەکار ئەھێنرێت بۆ ئاماژەدان بەھەر تەنێکی ئاسمانی یان دەستکرد، کە بە خولگەیێکی دیاری کراو بەدەوری زەویدا ئەسووڕێتەوە، یان ھەر ھەسارەیەکی تر، بۆ نموونە ھەسارەی کەیوان ھەژدە مانگی - پاشکۆی ھەیە.</p>
</article>");
            AddRtlSupport module = new AddRtlSupport();

            // When
            TestDocument result = await ExecuteAsync(document, module).SingleAsync();

            string expected = @"<html><head></head><body><h1 dir='ltr'>Hello World <span>سڵاو جیهان</span> Hello World</h1>
<p dir='ltr'>This is very nice.</p>
<div dir='rtl'>
  <p dir='rtl'>اهلا و سهلا</p>
  <p dir='ltr'>This is so cool</p>
</div>

<ul dir='rtl'>
<li>بەڵێ</li>
<li>Yes</li>
</ul>

<article dir='rtl'>
<h2 dir='rtl'>مانگ</h2>
<p dir='rtl'>مانگ ھەروەھا پێی دەوتریت ھەیڤ (بە ئینگلیزی: Moon، بە عەرەبی: القمر) بەکار ئەھێنرێت بۆ ئاماژەدان بەھەر تەنێکی ئاسمانی یان دەستکرد، کە بە خولگەیێکی دیاری کراو بەدەوری زەویدا ئەسووڕێتەوە، یان ھەر ھەسارەیەکی تر، بۆ نموونە ھەسارەی کەیوان ھەژدە مانگی - پاشکۆی ھەیە.</p>
</article></body></html>".Replace("'", "\"");

            // Then
            result.Content.ShouldBe(expected, StringCompareShould.IgnoreLineEndings);
        }

        [Test]
        public async Task AddProperDirectionAttributesToRtlTable()
        {
            // Given
            TestDocument document = new TestDocument(@"
<table>
  <tr>
    <th>ناو</th>
    <th>تەمەن</th>
  </tr>
  <tr>
    <td>ئاراس</td>
    <td>50</td>
  </tr>
  <tr>
    <td>هەڵۆ</td>
    <td>94</td>
  </tr>
</table>");
            AddRtlSupport module = new AddRtlSupport();

            // When
            TestDocument result = await ExecuteAsync(document, module).SingleAsync();

            string expected = @"<html><head></head><body><table dir='rtl' align='right'>
  <tbody><tr>
    <th>ناو</th>
    <th>تەمەن</th>
  </tr>
  <tr>
    <td>ئاراس</td>
    <td>50</td>
  </tr>
  <tr>
    <td>هەڵۆ</td>
    <td>94</td>
  </tr>
</tbody></table></body></html>".Replace("'", "\"");

            // Then
            result.Content.ShouldBe(expected, StringCompareShould.IgnoreLineEndings);
        }
    }
}
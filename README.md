# XKCDColourParser
The aim of this is to parse the original XKCD colour survey [text file](Output/original.txt) to a set of static Unity Color
properties, for use inside of Unity projects. Formatted versions of it exist in [C#6.0](Output/formatted_c%236.cs) and
[pre C#6.0](Output/formatted.cs) formats, for Unity projects who do not support C#6.0 syntax yet. The only difference between
both is the usage of C#6.0 is solely readonly getter properties, and the pre version uses readonly fields. Unlike the
[other version](http://forum.unity3d.com/threads/xkcd-colors-in-unity.85896/) of this file, this does not calculate and create new colours every single time you call the property, so it's slightly faster. XML comments have also been added with the RGB
code of each colours within them.

---
### License
The MIT License (MIT)

Copyright (c) 2016 Christophe Savard

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

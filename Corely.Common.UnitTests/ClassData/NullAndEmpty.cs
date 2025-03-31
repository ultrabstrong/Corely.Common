﻿using System.Collections;

namespace Corely.Common.UnitTests.ClassData;

public class NullAndEmpty : IEnumerable<object[]>
{
    private readonly List<object[]> _data =
    [
        [null],
            [string.Empty],
        ];

    public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

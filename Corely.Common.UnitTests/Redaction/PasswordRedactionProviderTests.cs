﻿using Corely.Common.Providers.Redaction;

namespace Corely.Common.UnitTests.Redaction;

public class PasswordRedactionProviderTests
{
    private readonly PasswordRedactionProvider _passwordRedactionProvider = new();

    [Theory]
    [MemberData(nameof(RedactTestData))]
    public void Redact_RedactsPassword(string input, string expected)
    {
        var actual = _passwordRedactionProvider.Redact(input);
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> RedactTestData() =>
    [
        [
                @"{""UserId"":1,""Username"":""username"",""Password"":""as@#$%#$^   09u09a8s09fj;qo34\""808+_)(*&^%$@!$#@^""",
                @"{""UserId"":1,""Username"":""username"",""Password"":""REDACTED"""
            ],
            [
                @"{""UserId"":1,""Username"":""username"",""Pwd"":""as@#$%#$^   09u09a8s09fj;qo34\""808+_)(*&^%$@!$#@^""",
                @"{""UserId"":1,""Username"":""username"",""Pwd"":""REDACTED"""
            ],
            [
                @"{ UserId: 0, Username: ""bstrong"", Password: ""as@#$%#$^   09u09a8s09fj;qo34\u0022808\u002B_)(*\u0026^%$@!$#@^"" }",
                @"{ UserId: 0, Username: ""bstrong"", Password: ""REDACTED"" }"
            ],
            [
                @"""UpsertBasicAuthRequest { UserId = 0, Username = bstrong, Password = as@#$%#$^09u09a8s09fj;qo34\u0022808\u002B_)(*\u0026^%$@!$#@^ }""",
                @"""UpsertBasicAuthRequest { UserId = 0, Username = bstrong, Password = REDACTED }"""
            ]
    ];
}

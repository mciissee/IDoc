using NUnit.Framework;
using System;
using Idoc.Lib;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework.Internal;

namespace Idoc.Test
{
    [TestFixture()]
    public class CSGrammarTest
    {
        private Lexer lexer;

        [SetUp]
        public void Init()
        {
            lexer = Lexer.CS;
        }

        [Test]
        public void LexerShouldBeLoaded()
        {
            Assert.IsTrue(lexer.Patterns.Any());
        }

        [TestCase("namespace A", "A", true)]
        public void ShouldExtractNamespace(string code, string name, bool success)
        {
            var match = lexer.FindPattern(Pattern.Namespace).Match(code);
            Assert.AreEqual(success, match.Success);
            Assert.AreEqual(name, match.Groups["name"].Value);
        }

        [TestCase("class A {", "A", true)]
        [TestCase("class A", "", false)]
        [TestCase("public class A<T>{", "A<T>", true)]
        [TestCase("internal class A<T, U>{", "A<T, U>", true)]
        [TestCase("public static class A<T> : List<T> {", "A<T>", true)]
        [TestCase("public static class A<T> : List<T>, ISerializable {", "A<T>", true)]
        [TestCase("public static class A<T> : List<T> ISerializable", "", false)]
        [TestCase("sealed class A<T> where T : class {", "A<T>", true)]
        public void ShouldExtractClass(string code, string name, bool success)
        {
            var match = lexer.FindPattern(Pattern.Class).Match(code);
            Assert.AreEqual(success, match.Success);
            Assert.AreEqual(name, match.Groups["name"].Value.Trim());
        }


        [TestCase("void F();", "F", "void", 0)]
        [TestCase(@"[Attribute(""void test()"")]void F();", "F", "void", 0)]
        [TestCase("int* F(ref int a){}", "F", "int*", 1)]
        [TestCase("char? F<A>(int a, char* c){}", "F<A>", "char?", 2)]
        [TestCase("char[][] F<A, B>(){}", "F<A, B>", "char[][]", 0)]
        [TestCase("A<B> F<A<B>>(){}", "F<A<B>>", "A<B>", 0)]
        [TestCase("public void F(int arg=10){}", "F", "void", 1)]
        [TestCase("internal static void F(params int[] args){}", "F", "void", 1)]
        [TestCase(@"void F(string s = ""...{},()"");", "F", "void", 1)]
        [TestCase("(int, int)[] F([Id()] ref int arg){}", "F", "(int, int)[]", 1)]
        [TestCase("return F();", "", "", 0)]
        [TestCase("new F();", "", "", 0)]
        [TestCase("F();", "F", "", 0)]
        [TestCase("int F() => 0;", "F", "int", 0)]
        [TestCase("int F() =>", "", "", 0)]
        [TestCase("int F() {", "", "", 0)]
        [TestCase("public static int operator+(A a, B b) => 0;", "operator+", "int", 2)]
        [TestCase("public static implicit operator (int, char)(A a) => (0, '');", "operator (int, char)", "implicit", 1)]
        public void ShouldExtractMethod(string code, string name, string type, int @params)
        {
            var match = lexer.FindPattern(Pattern.Method).Match(code);
            Assert.AreEqual(type, match.Groups["methodtype"].Value.Trim());
            Assert.AreEqual(name, match.Groups["methodname"].Value.Trim());
            Assert.AreEqual(@params, match.Groups["param"].Captures.Count);
        }


        [TestCase("int f", "", "")]
        [TestCase("int f = 10;", "f", "int")]
        [TestCase("int f;", "f", "int")]
        [TestCase("int? f = null;", "f", "int?")]
        [TestCase("int[] f;", "f", "int[]")]
        [TestCase("List<int> f;", "f", "List<int>")]
        [TestCase("List<int, List<char>> f;", "f", "List<int, List<char>>")]
        [TestCase("(int, int) f;", "f", "(int, int)")]
        public void ShouldExtractField(string code, string name, string type)
        {
            var match = lexer.FindPattern(Pattern.Field).Match(code);
            Assert.AreEqual(type, match.Groups["fieldtype"].Value.Trim());
            Assert.AreEqual(name, match.Groups["fieldname"].Value.Trim());
        }

        [TestCase("int p;", "", "")]
        [TestCase("int p => 0;", "p", "int")]
        [TestCase("int p { get; set; }", "p", "int")]
        [TestCase("int this[int index] { get; set; }", "this[int index]", "int")]
        public void ShouldExtractProperty(string code, string name, string type)
        {
            var match = lexer.FindPattern(Pattern.Property).Match(code);
            Assert.AreEqual(type, match.Groups["proptype"].Value.Trim());
            Assert.AreEqual(name, match.Groups["propname"].Value.Trim());
        }


    }
}

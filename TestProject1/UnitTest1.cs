using Microsoft.VisualStudio.TestTools.UnitTesting;
using CGX_Formatter;
using System.Linq;
using System.IO;

namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Tokenizer tokenizer = new Tokenizer();
            var tokens = tokenizer.GetTokens("(('k1'=1);('k2'=2))");
            Assert.AreEqual(13, tokens.Count());
        }
        [TestMethod]
        public void TestMethod2()
        {
            Tokenizer tokenizer = new Tokenizer();
            var tokens = tokenizer.GetTokens("(0;1;2)");
            Assert.AreEqual(7, tokens.Count());
        }
        [TestMethod]
        public void TestMethod3()
        {
            Tokenizer tokenizer = new Tokenizer();
            var tokens = tokenizer.GetTokens("true");
            Assert.AreEqual(1, tokens.Count());
        }
        [TestMethod]
        public void TestMethod4()
        {
            Tokenizer tokenizer = new Tokenizer();
            var tokens = tokenizer.GetTokens("( )");
            Assert.AreEqual(2, tokens.Count());
        }
        [TestMethod]
        public void TestMethod5()
        {
            Tokenizer tokenizer = new Tokenizer();
            var tokens = tokenizer.GetTokens("'spaces and    tabs'");
            Assert.AreEqual(1, tokens.Count());
        }

        [TestMethod]
        public void TestMethod6()
        {
            Parser parser= new Parser();
            var result = parser.Parse("(('k1'=1);('k2'=2))");
            var formation = result.ToString(0);
            File.WriteAllText("t6.txt", formation);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod7()
        {
            Parser parser = new Parser();
            var result = parser.Parse(@"(
    'menu'=
    (
        'id'= 'file';
        'value'= 'File';
        'popup'=
        (
            'menuitem'=
            (
                ( 'value'= 'New'; 'onclick'= 'CreateNewDoc()' );
                ( 'value'= 'Open'; 'onclick'= 'OpenDoc()' );
                ( 'value'= 'Close'; 'onclick'= 'CloseDoc()' )
            )
        ); ()
    )
)
");
            var formation = result.ToString(0);
            File.WriteAllText("t7.txt", formation);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod8()
        {
            Parser parser = new Parser();
            var result = parser.Parse(@"
'users'=(('id'=10;
'name'='Serge';
'roles'=('visitor';
'moderator'
));
('id'=11;
'name'='Biales'
);
true
)
");
            var formation = result.ToString(0);
            File.WriteAllText("t8.txt", formation);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod9()
        {
            Parser parser = new Parser();
            var result = parser.Parse(@"
( 'user'= (
    'key'='1= t(c)(';
    'valid'=false
  );
  'user'= (
    'key'=' = ; ';
    'valid'= true
  ); ()
​)
");
            var formation = result.ToString(0);
            File.WriteAllText("t9.txt", formation);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod10()
        {
            Parser parser = new Parser();
            var result = parser.Parse("true");
            var formation = result.ToString(0);
            File.WriteAllText("t10.txt", formation);
            Assert.IsNotNull(result);
        }

//        [TestMethod]
//        public void TestMethod11()
//        {
//            Parser parser = new Parser();
//            bool result = parser.Parse(@"
//((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((
//0
//))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))
//");
//            Assert.AreEqual(result, true);
//        }

        [TestMethod]
        public void TestMethod12()
        {
            Parser parser = new Parser();
            var result = parser.Parse("(1=1)");
            Assert.IsNull(result);
        }
    }
}

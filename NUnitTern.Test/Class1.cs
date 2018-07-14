using NUnit.Framework;
using System;

namespace TestProjectToMigrate
{
    public class Class1
    {
        [Test]
        public void WithException()
        {
            Assert.Throws<Exception>(() =>
            {
                var toto = 1 * 2;
                DoNothing();
            });
        }

        public void DoNothing() { }

    //    [Test, RequiresMTA]
    //    public void Yellow()
    //    {
    //        Assert.IsNotNullOrEmpty("MyString", "MyErrorMessage");
    //    }

    //    [TestCase(Result = "Something")]
    //    public void Red()
    //    {
    //        Assert.IsNull(new object());
    //    }

    //    [TestCase(Ignore = true)]
    //    public void AnIgnoreMethod() { }
    //}

    //[TestFixture(Ignore = true)]
    //public class Class2
    //{
    //    [Test]
    //    public void ATest()
    //    {
    //        Assert.That("Toto", Text.Contains("to"));
    //        Assert.That("Toto", Is.StringStarting("to"));
    //    }
    }
}

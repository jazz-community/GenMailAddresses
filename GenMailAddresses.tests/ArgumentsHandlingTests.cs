using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GenMailAddresses.Test1
{
    [TestFixture]
    public class ArgumentsHandlingTests
    {
        [Test]
        //[ExpectedException(typeof(NoArgumentsException)]
        public void T01_ArgumentHandling()
        {
            string[] a = new string[0];
            //Options o = ArgumentsHandling.getOptionsFromArguments(a);
            Assert.That(() => ArgumentsHandling.getOptionsFromArguments(a),
               Throws.TypeOf<NoArgumentsException>());
        }

        [Test]
        //[ExpectedException(typeof(ArgumentsParseException), ExpectedMessage = "Unknown option: all")]
        public void T02_ArgumentHandling()
        {
            string[] a = new string[] { "-url", "URL", "--area", "AREA", "-all" };
            //Options o = ArgumentsHandling.getOptionsFromArguments(a);
            Assert.That(() => ArgumentsHandling.getOptionsFromArguments(a),
               Throws.TypeOf<ArgumentsParseException>().With.Message.EqualTo("Unknown option: all"));
        }

        [Test]
        //[ExpectedException(typeof(ArgumentsParseException), ExpectedMessage = "Stray argument: wuff")]
        public void T03_ArgumentHandling()
        {
            string[] a = new string[] { "-url", "xox-xox-xx", "wuff", "--area", "AREA" };
            //Options o = ArgumentsHandling.getOptionsFromArguments(a);
            Assert.That(() => ArgumentsHandling.getOptionsFromArguments(a),
               Throws.TypeOf<ArgumentsParseException>().With.Message.EqualTo("Stray argument: wuff"));
        }

        [Test]
        //[ExpectedException(typeof(ArgumentsParseException), ExpectedMessage = "Missing parameter '--area' OR option '-a'")]
        public void T04_ArgumentHandling()
        {
            string[] a = new string[] { "-url", "xox-xox-xx" };
            //Options o = ArgumentsHandling.getOptionsFromArguments(a);
            Assert.That(() => ArgumentsHandling.getOptionsFromArguments(a),
               Throws.TypeOf<ArgumentsParseException>().With.Message.EqualTo("Missing parameter '--area' OR option '-a'"));
        }

        [Test]
        //[ExpectedException(typeof(ArgumentsParseException), ExpectedMessage = "Missing value for parameter: area")]
        public void T05_ArgumentHandling()
        {
            string[] a = new string[] { "-url", "xox-xox-xx", "--area" };
            //Options o = ArgumentsHandling.getOptionsFromArguments(a);
            Assert.That(() => ArgumentsHandling.getOptionsFromArguments(a),
               Throws.TypeOf<ArgumentsParseException>().With.Message.EqualTo("Missing value for parameter: area"));
        }

        [Test]
        public void T06_ArgumentHandling()
        {
            string[] a = new string[] { "-url", "https://jazz.rsint.net/ccm/web", "--area", "ayay" };
            Options o = ArgumentsHandling.getOptionsFromArguments(a);
            Assert.IsFalse(o.opt_recursive);
            Assert.IsFalse(o.opt_verbose);
            Assert.IsFalse(o.opt_all_areas);
            Assert.That(o.jazzRepository == "https://jazz.rsint.net");
            Assert.IsNull(o.user);
            Assert.IsNull(o.passwd);
            Assert.That(o.area == "ayay");
        }

        [Test]
        public void T07_ArgumentHandling()
        {
            string[] a = new string[] { "-url", "https://jazz.rsint.net/ccm/web", "--area", "ayay", "--r" };
            Options o = ArgumentsHandling.getOptionsFromArguments(a);
            Assert.IsTrue(o.opt_recursive);
            Assert.IsFalse(o.opt_verbose);
            Assert.IsFalse(o.opt_all_areas);
            Assert.That(o.jazzRepository == "https://jazz.rsint.net");
            Assert.IsNull(o.user);
            Assert.IsNull(o.passwd);
            Assert.That(o.area == "ayay");
            Assert.IsNull(o.roles);
        }

        [Test]
        //[ExpectedException(typeof(ArgumentsParseException), ExpectedMessage = "Stray argument: superfluous")]
        public void T08_ArgumentHandling()
        {
            string[] a = new string[] { "-url", "https://jazz.rsint.net/ccm/web", "--area", "ayay", "--r", "superfluous" };
            //Options o = ArgumentsHandling.getOptionsFromArguments(a);
            Assert.That(() => ArgumentsHandling.getOptionsFromArguments(a),
               Throws.TypeOf<ArgumentsParseException>().With.Message.EqualTo("Stray argument: superfluous"));
        }

        [Test]
        //[ExpectedException(typeof(ArgumentsParseException), ExpectedMessage = "Stray argument: all")]
        public void T09_ArgumentHandling()
        {
            string[] a = new string[] { "all", "-url", "URL", "--area", "AREA"};
            //Options o = ArgumentsHandling.getOptionsFromArguments(a);
            Assert.That(() => ArgumentsHandling.getOptionsFromArguments(a),
               Throws.TypeOf<ArgumentsParseException>().With.Message.EqualTo("Stray argument: all"));
        }

        [Test]
        public void T10_ArgumentHandling()
        {
            string[] a = new string[] { "-url", "https://jazz.rsint.net/ccm/web", "--area", "ayay", "--r", "--role", "R1", "R2"};
            Options o = ArgumentsHandling.getOptionsFromArguments(a);
            Assert.IsTrue(o.opt_recursive);
            Assert.IsFalse(o.opt_verbose);
            Assert.IsFalse(o.opt_all_areas);
            Assert.That(o.jazzRepository == "https://jazz.rsint.net");
            Assert.IsNull(o.user);
            Assert.IsNull(o.passwd);
            Assert.That(o.area == "ayay");
            Assert.That(o.roles.Count == 2);
        }

        [Test]
        public void T11_ArgumentHandling()
        {
            string[] a = new string[] { "-url", "https://jazz.rsint.net/ccm/web", "--area", "ayay", "--r", "--role", "R1", "R2", "-role", "R3" };
            Options o = ArgumentsHandling.getOptionsFromArguments(a);
            Assert.IsTrue(o.opt_recursive);
            Assert.IsFalse(o.opt_verbose);
            Assert.IsFalse(o.opt_all_areas);
            Assert.That(o.jazzRepository == "https://jazz.rsint.net");
            Assert.IsNull(o.user);
            Assert.IsNull(o.passwd);
            Assert.That(o.area == "ayay");
            Assert.That(o.roles.Count == 3);
        }

        [Test]
        //[ExpectedException(typeof(ArgumentsParseException), ExpectedMessage = "Either specify area path OR -a but not both")]
        public void T12_ArgumentHandling()
        {
            string[] a = new string[] { "-url", "xox-xox-xx", "--area", "AREA", "-a" };
            //Options o = ArgumentsHandling.getOptionsFromArguments(a);
            Assert.That(() => ArgumentsHandling.getOptionsFromArguments(a),
               Throws.TypeOf<ArgumentsParseException>().With.Message.EqualTo("Either specify area path OR -a but not both"));
        }

        [Test]
        public void T13_ArgumentHandling()
        {
            string[] a = new string[] { "-url", "xox-xox-xx", "-a" };
            Options o = ArgumentsHandling.getOptionsFromArguments(a);
            Assert.IsTrue(o.opt_all_areas);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using DotNetWebToolkit.Web;

namespace Test.BrowserTests.Window_ {

    [TestFixture]
    public class TestTimers : BrowserTestBase {

        [Test]
        public void TestSetInterval() {
            Action f = () => {
                int i = 0;
                Window.SetInterval(() => {
                    if (i++ == 2) {
                        Pass();
                    }
                }, 1);
            };
            this.Start(f);
        }

        [Test]
        public void TestSetIntervalTimespan() {
            Action f = () => {
                var timeout = TimeSpan.FromMilliseconds(5);
                int i = 0;
                Window.SetInterval(() => {
                    if (i++ == 2) {
                        Pass();
                    }
                }, timeout);
            };
            this.Start(f);
        }

        [Test]
        public void TestClearInterval() {
            Action f = () => {
                var id = Window.SetInterval(() => Fail(), 2);
                Window.ClearInterval(id);
                Window.SetInterval(() => Pass(), 50);
            };
            this.Start(f);
        }

        [Test]
        public void TestSetTimeout() {
            Action f = () => {
                int i = 0;
                Window.SetTimeout(() => i++, 1);
                Window.SetTimeout(() => Done(i == 1), 50);
            };
            this.Start(f);
        }

        [Test]
        public void TestSetTimeoutTimespan() {
            Action f = () => {
                int i = 0;
                Window.SetTimeout(() => i++, TimeSpan.FromMilliseconds(1));
                Window.SetTimeout(() => Done(i == 1), TimeSpan.FromMilliseconds(50));
            };
            this.Start(f);
        }

        [Test]
        public void TestClearTimeout() {
            Action f = () => {
                int i=0;
                var id = Window.SetTimeout(() => i++, 1);
                Window.ClearTimeout(id);
                Window.SetTimeout(() => Done(i == 0), 50);
            };
            this.Start(f);
        }

    }

}

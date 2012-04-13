﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestNumericEquality : ExecutionTestBase {

        [Test]
        public void TestInt8EqualsObject() {
            Func<bool> f = () => ((sbyte)32).Equals((object)(sbyte)32) && !((sbyte)32).Equals((object)(sbyte)33) && !((sbyte)32).Equals((object)32);
            this.Test(f, true);
        }

        [Test]
        public void TestInt8EqualsInt8() {
            Func<bool> f = () => ((sbyte)32).Equals((sbyte)32) && !((sbyte)32).Equals((sbyte)33);
            this.Test(f, true);
        }

        [Test]
        public void TestUInt8EqualsObject() {
            Func<bool> f = () => ((byte)32).Equals((object)(byte)32) && !((byte)32).Equals((object)(byte)33) && !((byte)32).Equals((object)32);
            this.Test(f, true);
        }

        [Test]
        public void TestUInt8EqualsUInt8() {
            Func<bool> f = () => ((byte)32).Equals((byte)32) && !((byte)32).Equals((byte)33);
            this.Test(f, true);
        }

        [Test]
        public void TestInt16EqualsObject() {
            Func<bool> f = () => ((short)32).Equals((object)(short)32) && !((short)32).Equals((object)(short)33) && !((short)32).Equals((object)32);
            this.Test(f, true);
        }

        [Test]
        public void TestInt16EqualsInt16() {
            Func<bool> f = () => ((short)32).Equals((short)32) && !((short)32).Equals((short)33);
            this.Test(f, true);
        }

        [Test]
        public void TestUInt16EqualsObject() {
            Func<bool> f = () => ((ushort)32).Equals((object)(ushort)32) && !((ushort)32).Equals((object)(ushort)33) && !((ushort)32).Equals((object)32);
            this.Test(f, true);
        }

        [Test]
        public void TestUInt16EqualsUInt16() {
            Func<bool> f = () => ((ushort)32).Equals((ushort)32) && !((ushort)32).Equals((ushort)33);
            this.Test(f, true);
        }

        [Test]
        public void TestInt32EqualsObject() {
            Func<bool> f = () => (32).Equals((object)32) && !(32).Equals((object)33) && !(32).Equals((object)32L);
            this.Test(f, true);
        }

        [Test]
        public void TestInt32EqualsInt32() {
            Func<bool> f = () => (32).Equals(32) && !(32).Equals(33);
            this.Test(f, true);
        }

        [Test]
        public void TestUInt32EqualsObject() {
            Func<bool> f = () => (32U).Equals((object)32U) && !(32U).Equals((object)33U) && !(32U).Equals((object)32);
            this.Test(f, true);
        }

        [Test]
        public void TestUInt32EqualsUInt32() {
            Func<bool> f = () => (32U).Equals(32U) && !(32U).Equals(33U);
            this.Test(f, true);
        }

        [Test]
        public void TestInt64EqualsObject() {
            Func<bool> f = () => (32L).Equals((object)32L) && !(32L).Equals((object)33L) && !(32L).Equals((object)32);
            this.Test(f, true);
        }

        [Test]
        public void TestInt64EqualsInt64() {
            Func<bool> f = () => (32L).Equals(32L) && !(32L).Equals(33L);
            this.Test(f, true);
        }

        [Test]
        public void TestUInt64EqualsObject() {
            Func<bool> f = () => (32UL).Equals((object)32UL) && !(32UL).Equals((object)33UL) && !(32UL).Equals((object)32);
            this.Test(f, true);
        }

        [Test]
        public void TestUInt64EqualsUInt64() {
            Func<bool> f = () => (32UL).Equals(32UL) && !(32UL).Equals(33UL);
            this.Test(f, true);
        }

        [Test]
        public void TestSingleEqualsObject() {
            Func<bool> f = () => (32f).Equals((object)32f) && !(32f).Equals((object)33f) && !(32f).Equals((object)32);
            this.Test(f, true);
        }

        [Test]
        public void TestSingleEqualsSingle() {
            Func<bool> f = () => (32f).Equals(32f) && !(32f).Equals(33f);
            this.Test(f, true);
        }

        [Test]
        public void TestDoubleEqualsObject() {
            Func<bool> f = () => (32d).Equals((object)32d) && !(32d).Equals((object)33d) && !(32d).Equals((object)32);
            this.Test(f, true);
        }

        [Test]
        public void TestDoubleEqualsSingle() {
            Func<bool> f = () => (32d).Equals(32d) && !(32d).Equals(33d);
            this.Test(f, true);
        }

    }

}

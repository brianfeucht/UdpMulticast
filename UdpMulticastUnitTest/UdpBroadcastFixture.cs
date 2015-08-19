using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UdpMulticast;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace UdpMulticastUnitTest
{
    [TestClass]
    public class UdpBroadcastFixture
    {
        public class TestMessage
        {
            public string Text { get; set; }
            public bool Bit { get; set; }
            public int Number { get; set; }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                if(obj is TestMessage)
                {
                    var value = obj as TestMessage;

                    return value.Text == Text && value.Bit == Bit && value.Number == Number;
                }

                return base.Equals(obj);
            }
        }

        [TestMethod]
        public async Task OneSendOneRecieve()
        {
            var wasRecieved = false;
            var reciever = new UdpReciever<TestMessage>((value) => { wasRecieved = true; });
            reciever.Start();
            await Task.Delay(20);

            var sender = new UdpBroadcaster();

            await sender.Send(new TestMessage());

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            while(!wasRecieved)
            {
                cts.Token.ThrowIfCancellationRequested();
                await Task.Delay(10, cts.Token);
            }

            reciever.Stop();

            Assert.IsTrue(wasRecieved);
        }


        [TestMethod]
        public async Task TenSendsTenRecieves()
        {
            int expected = 10;
            int recieves = 0;
            var reciever = new UdpReciever<TestMessage>((value) => { Interlocked.Increment(ref recieves); });
            reciever.Start();
            await Task.Delay(20);

            var sender = new UdpBroadcaster();

            for (int i = 0; i < expected; i++)
            {
                await sender.Send(new TestMessage());
            }

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            while (recieves < 10)
            {
                cts.Token.ThrowIfCancellationRequested();
                await Task.Delay(10, cts.Token);
            }

            reciever.Stop();

            Assert.AreEqual(expected, recieves);
        }

        [TestMethod]
        public async Task EntireMessageReceived()
        {
            var expected = new TestMessage()
            {
                Text = "Some text goes here",
                Bit = false,
                Number = 1245423534
            };

            TestMessage actual = null;

            var reciever = new UdpReciever<TestMessage>((value) => { actual = value; });
            reciever.Start();
            await Task.Delay(20);

            var sender = new UdpBroadcaster();

            await sender.Send(expected);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            while (actual == null)
            {
                cts.Token.ThrowIfCancellationRequested();
                await Task.Delay(10, cts.Token);
            }

            reciever.Stop();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public async Task TwoReceivers()
        {
            var expected = new TestMessage()
            {
                Text = "Some text goes here",
                Bit = false,
                Number = 1245423534
            };

            TestMessage actual1 = null;

            var reciever1 = new UdpReciever<TestMessage>((value) => { actual1 = value; });
            reciever1.Start();


            TestMessage actual2 = null;

            var reciever2 = new UdpReciever<TestMessage>((value) => { actual2 = value; });
            reciever2.Start();

            await Task.Delay(20);

            var sender = new UdpBroadcaster();

            await sender.Send(expected);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            while (actual1 == null || actual2 == null)
            {
                cts.Token.ThrowIfCancellationRequested();
                await Task.Delay(10, cts.Token);
            }

            reciever1.Stop();
            reciever2.Stop();

            Assert.AreEqual(expected, actual1);
            Assert.AreEqual(expected, actual2);
        }

        [TestMethod]
        public async Task EntireMessageReceivedBig()
        {
            var expected = new TestMessage()
            {
                Text = GenerateText(65464),
                Bit = false,
                Number = 1245423534
            };

            TestMessage actual = null;

            var reciever = new UdpReciever<TestMessage>((value) => { actual = value; });
            reciever.Start();
            await Task.Delay(20);

            var sender = new UdpBroadcaster();

            await sender.Send(expected);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            while (actual == null)
            {
                cts.Token.ThrowIfCancellationRequested();
                await Task.Delay(10, cts.Token);
            }

            reciever.Stop();

            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public async Task EntireMessageReceivedTooBig()
        {
            var expected = new TestMessage()
            {
                Text = GenerateText(65465),
                Bit = false,
                Number = 1245423534
            };

            TestMessage actual = null;

            var reciever = new UdpReciever<TestMessage>((value) => { actual = value; });
            reciever.Start();
            await Task.Delay(20);

            var sender = new UdpBroadcaster();

            await sender.Send(expected);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            while (actual == null)
            {
                cts.Token.ThrowIfCancellationRequested();
                await Task.Delay(10, cts.Token);
            }

            reciever.Stop();

            Assert.AreEqual(expected, actual);
        }

        private string GenerateText(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }
    }
}

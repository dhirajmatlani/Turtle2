using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Turtle;
using System.Collections.Generic;

namespace Test
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            Parser parser = new Parser();
            IDisplay display = new MockGraphics();

            Command command = new Command(display);
            Position currentPosition = new Position();

            List<CommandPayload> commandPayloads = parser.Parse("PLACE 0,0,NORTH");
            foreach (CommandPayload commandPayload in commandPayloads)
            {
                currentPosition = command.Execute(commandPayload);
            }

            commandPayloads = parser.Parse("LEFT");
            foreach (CommandPayload commandPayload in commandPayloads)
            {
                currentPosition = command.Execute(commandPayload);
            }

            commandPayloads = parser.Parse("REPORT");
            foreach (CommandPayload commandPayload in commandPayloads)
            {
                currentPosition = command.Execute(commandPayload);
            }

            Assert.AreEqual(0, currentPosition.Coordinates.X);
            Assert.AreEqual(0, currentPosition.Coordinates.Y);
            Assert.AreEqual(Direction.West, currentPosition.Direction);
        }

        public void TestMethod2()
        {

        }
    }

    public class MockGraphics : IDisplay
    {
        public string Display(Position position)
        {
            string display = position.Coordinates.X.ToString() + ","
                + position.Coordinates.Y + ","
                + position.Direction.ToString();
            return display;
        }
    }
}

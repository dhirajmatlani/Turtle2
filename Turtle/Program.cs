using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turtle
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser parser = new Parser();
            IDisplay display = new Graphics();
            Command command = new Command(display);
            while (true)
            {
                string input = Console.ReadLine();
                List<CommandPayload> commandPayloads = parser.Parse(input);
                foreach(CommandPayload commandPayload in commandPayloads)
                {
                    command.Execute(commandPayload);
                }
            }
        }
    }

    public interface IParse
    {
        List<CommandPayload> Parse(string input);
    }

    public class Parser : IParse
    {
        public List<CommandPayload> Parse(string input)
        {
            List<CommandPayload> commandPayloads = new List<CommandPayload>();
            if(input.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase))
            {
                string[] lines = File.ReadAllLines(input);
                foreach(string line in lines)
                {
                    CommandPayload commandPayload = ParseCommandPayload(line);
                    commandPayloads.Add(commandPayload);
                }
            }
            else
            {
                CommandPayload commandPayload = ParseCommandPayload(input);
                commandPayloads.Add(commandPayload);
            }
            
            return commandPayloads;
        }

        public CommandPayload ParseCommandPayload(string payload)
        {
            CommandPayload commandPayload = new CommandPayload();
            string[] payloadSubset = payload.Split(new char[] { ' ' });
            string action = null;
            if (payloadSubset == null || payloadSubset.Length == 0)
            {
                action = payload;
            }
            else
            {
                action = payloadSubset[0];
            }
            
            if (0 == string.Compare(action, "Place", true))
            {
                string[] position = payloadSubset[1].Split(new char[] { ',' });

                commandPayload.Action = Action.Place;
                commandPayload.Position = new Position();
                
                commandPayload.Position.Coordinates.X = Convert.ToInt32(position[0]);
                commandPayload.Position.Coordinates.Y = Convert.ToInt32(position[1]);
                if (0 == string.Compare("East", position[2], true))
                {
                    commandPayload.Position.Direction = Direction.East;
                }
                else if (0 == string.Compare("West", position[2], true))
                {
                    commandPayload.Position.Direction = Direction.West;
                }
                else if (0 == string.Compare("North", position[2], true))
                {
                    commandPayload.Position.Direction = Direction.North;
                }
                else if (0 == string.Compare("South", position[2], true))
                {
                    commandPayload.Position.Direction = Direction.South;
                }
            }
            else if (0 == string.Compare(action, "Move", true))
            {
                commandPayload.Action = Action.Move;
            }
            else if (0 == string.Compare(action, "Left", true))
            {
                commandPayload.Action = Action.Left;
            }
            else if (0 == string.Compare(action, "Right", true))
            {
                commandPayload.Action = Action.Right;
            }
            else if (0 == string.Compare(action, "Report", true))
            {
                commandPayload.Action = Action.Report;
            }

            return commandPayload;
        }
    }

    public interface ICommand
    {
        Position Execute(CommandPayload commandPayload);
    }

    public class Command : ICommand
    {
        PositionCalculator positionCalculator = new PositionCalculator();
        PositionValidator positionValidator = new PositionValidator();
        IDisplay graphics;

        public Command()
        {

        }

        public Command(IDisplay display)
        {
            graphics = display;
        }
        public Position Execute(CommandPayload commandPayload)
        {
            switch(commandPayload.Action)
            {
                case Action.Place:
                case Action.Move:
                case Action.Left:
                case Action.Right:
                    positionCalculator.Calculate(commandPayload);
                    break;
                case Action.Report:
                    if(positionValidator.IsValid(positionCalculator.currentPosition.Coordinates) &&
                        positionValidator.IsValid(positionCalculator.currentPosition.Direction))
                    {
                        graphics.Display(positionCalculator.currentPosition);
                    }
                    break;
                default:
                    break;
            }
            return positionCalculator.currentPosition;
        }
    } 

    public interface IDisplay
    {
        string Display(Position position);
    }

    public class Graphics : IDisplay
    {
        public string Display(Position position)
        {
            string display = position.Coordinates.X.ToString() + ","
                + position.Coordinates.Y + ","
                + position.Direction.ToString();
            Console.WriteLine(display);
            return display;
        }
    }

    public interface ICalculate
    {
        Position Calculate(CommandPayload commandPayload);
    }

    public class PositionCalculator : ICalculate
    {
        Dictionary<Direction, Dictionary<Action, Direction>> positions;
        public Position currentPosition;
        PositionValidator positionValidator;

        public PositionCalculator()
        {
            Coordinates coordinates = new Coordinates();
            coordinates.X = -1;
            coordinates.Y = -1;
            Direction direction = Direction.NoWhere;
            currentPosition = new Position(coordinates, direction);
            positions = new Dictionary<Direction, Dictionary<Action, Direction>>();

            Dictionary<Action, Direction> leftRightNorthSouth = new Dictionary<Action, Direction>();
            leftRightNorthSouth.Add(Action.Left, Direction.North);
            leftRightNorthSouth.Add(Action.Right, Direction.South);
            positions.Add(Direction.East, leftRightNorthSouth);

            Dictionary<Action, Direction> leftRightSouthNorth = new Dictionary<Action, Direction>();
            leftRightSouthNorth.Add(Action.Left, Direction.South);
            leftRightSouthNorth.Add(Action.Right, Direction.North);
            positions.Add(Direction.West, leftRightSouthNorth);

            Dictionary<Action, Direction> leftRightWestEast = new Dictionary<Action, Direction>();
            leftRightWestEast.Add(Action.Left, Direction.West);
            leftRightWestEast.Add(Action.Right, Direction.East);
            positions.Add(Direction.North, leftRightWestEast);

            Dictionary<Action, Direction> leftRightEastWest = new Dictionary<Action, Direction>();
            leftRightEastWest.Add(Action.Left, Direction.East);
            leftRightEastWest.Add(Action.Right, Direction.West);
            positions.Add(Direction.South, leftRightEastWest);
           
            positionValidator = new PositionValidator();

        }
        public Position Calculate(CommandPayload commandPayload)
        {
           
            switch(commandPayload.Action)
            {
                case Action.Place:
                    bool isValid = positionValidator.IsValid(commandPayload.Position.Coordinates);
                    if(isValid)
                    {
                        currentPosition.Coordinates = commandPayload.Position.Coordinates;
                        currentPosition.Direction = commandPayload.Position.Direction;
                    }
                    break;
                case Action.Move:
                    if (positionValidator.IsValid(currentPosition.Coordinates))
                    {
                        Coordinates newCoordinates = new Coordinates();
                        newCoordinates.X = currentPosition.Coordinates.X;
                        newCoordinates.Y = currentPosition.Coordinates.Y;

                        switch (currentPosition.Direction)
                        {
                            case Direction.East:
                                newCoordinates.X = newCoordinates.X + 1;
                                break;
                            case Direction.West:
                                newCoordinates.X = newCoordinates.X - 1;
                                break;
                            case Direction.North:
                                newCoordinates.Y = newCoordinates.Y + 1;
                                break;
                            case Direction.South:
                                newCoordinates.Y = newCoordinates.Y + 1;
                                break;
                            default:
                                break;
                        }
                        if (positionValidator.IsValid(newCoordinates))
                        {
                            currentPosition.Coordinates = newCoordinates;
                        }
                    }
                    break;
                case Action.Left:
                case Action.Right:
                    if (positionValidator.IsValid(currentPosition.Direction))
                    {
                        currentPosition.Direction = positions[currentPosition.Direction][commandPayload.Action];
                    }
                    break;

                default:
                    break;
            }
           
            return currentPosition;
        }
    }

    public interface IValidate
    {
        bool IsValid(Coordinates coordinates);
        bool IsValid(Direction direction);
    }

    public class PositionValidator : IValidate
    {
        public Coordinates MinCordinates { get; set; }

        public Coordinates MaxCoordinates { get; set; }
        public PositionValidator(Coordinates minCordinates, Coordinates maxCoordinates)
        {
            MinCordinates = minCordinates;
            MaxCoordinates = maxCoordinates;
        }

        public PositionValidator()
        {
            Coordinates minCoordinates = new Coordinates();
            minCoordinates.X = 0;
            minCoordinates.Y = 0;
            MinCordinates = minCoordinates;
            Coordinates maxCoordinates = new Coordinates();
            maxCoordinates.X = 4;
            maxCoordinates.Y = 4;
            MaxCoordinates = maxCoordinates;
        }

        public bool IsValid(Coordinates coordinates)
        {
            bool isValid = false;
            if (coordinates.X >= MinCordinates.X && coordinates.X <= MaxCoordinates.X)
            {
                if (coordinates.X >= MinCordinates.X && coordinates.X <= MaxCoordinates.X)
                {
                    isValid = true;
                }
            }
            return isValid;
        }

        public bool IsValid(Direction direction)
        {
            bool isValid = false;
            if(direction == Direction.East || direction == Direction.West || direction == Direction.North || direction == Direction.South)
            {
                isValid = true;
            }
            return isValid;
        }
    }
    
    public enum Direction
    {
        East,
        West,
        North,
        South,
        NoWhere
    }

    public enum Action
    {
        Place,
        Move,
        Left,
        Right,
        Report
    }

    public class CommandPayload
    {
        public Action Action { get; set; }

        public Position Position { get; set; }
    }

    public class Coordinates
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Position
    {
        public Direction Direction { get; set; }

        public Coordinates Coordinates { get; set; }

        public Position(Coordinates coordinates, Direction direction)
        {
            Coordinates = coordinates;
            Direction = Direction;
        } 

        public Position()
        {
            Coordinates = new Coordinates();
        }
    }
}

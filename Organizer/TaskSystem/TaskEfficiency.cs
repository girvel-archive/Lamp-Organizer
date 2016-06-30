using System;

namespace Organizer.TaskSystem {
    [Serializable]
    public class TaskEfficiency : CharEnum, IPortable {
        private const char 
            CanceledSymbol = 'c',
            NotDefinedSymbol = 'n',
            NullSymbol = '0',
            MinimalSymbol = '1',
            LowSymbol = '2',
            MediumSymbol = '3',
            NormalSymbol = '4',
            MaximalSymbol = '5';

        public static readonly TaskEfficiency 
            Canceled = new TaskEfficiency(CanceledSymbol),
            NotDefined = new TaskEfficiency(NotDefinedSymbol),
            Null = new TaskEfficiency(NullSymbol),
            Minimal = new TaskEfficiency(MinimalSymbol),
            Low = new TaskEfficiency(LowSymbol),
            Medium = new TaskEfficiency(MediumSymbol),
            Normal = new TaskEfficiency(NormalSymbol),
            Maximal = new TaskEfficiency(MaximalSymbol);

        public int Value {
            get {
                switch (Symbol) {
                    default:
                        throw new ArgumentException();

                    case CanceledSymbol:
                        return 0;

                    case NotDefinedSymbol:
                        return MaxValue;

                    case NullSymbol:
                        return 0;

                    case MinimalSymbol:
                        return 1;

                    case LowSymbol:
                        return 2;

                    case MediumSymbol:
                        return 3;

                    case NormalSymbol:
                        return 4;

                    case MaximalSymbol:
                        return 5;
                }
            }
        }

        public const int MaxValue = 5;

        protected TaskEfficiency(char symbol)
            : base(symbol) {}

        public static TaskEfficiency Parse(char efficiency) {
            switch (efficiency) {
                case CanceledSymbol:
                    return Canceled;

                case NotDefinedSymbol:
                    return NotDefined;

                case NullSymbol:
                    return Null;

                case MinimalSymbol:
                    return Minimal;

                case LowSymbol:
                    return Low;

                case MediumSymbol:
                    return Medium;

                case NormalSymbol:
                    return Normal;

                case MaximalSymbol:
                    return Maximal;

                default:
                    throw new FormatException();
            }
        }

        public void Port(ProgramVersion oldVersion) {
            
        }
    }
}
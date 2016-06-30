using System;

namespace Organizer.TaskSystem {
    [Serializable]
    public class TargetCondition : CharEnum, IPortable {
        private const char
            BlockedSymbol = 'x',
            FailedSymbol = '-',
            InProgressSymbol = '*',
            PartiallyCompletedSymbol = '~',
            CompletedSymbol = '+';

        public static readonly TargetCondition 
            Blocked = new TargetCondition(BlockedSymbol),
            Failed = new TargetCondition(FailedSymbol),
            InProgress = new TargetCondition(InProgressSymbol),
            PartiallyCompleted = new TargetCondition(PartiallyCompletedSymbol),
            Completed = new TargetCondition(CompletedSymbol);

        protected TargetCondition(char symbol)
            : base(symbol) {}

        public static TargetCondition Parse(char symbol) {
            switch (symbol) {
                case BlockedSymbol:
                    return Blocked;

                case FailedSymbol:
                    return Failed;

                case InProgressSymbol:
                    return InProgress;

                case PartiallyCompletedSymbol:
                    return PartiallyCompleted;

                case CompletedSymbol:
                    return Completed;
                
                default:
                    throw new ArgumentException();
            }
        }

        public void Port(ProgramVersion oldVersion) {
            if (oldVersion > ProgramVersion.Basic10) return;

            if (Symbol == '-')
                Symbol = BlockedSymbol;

            if (Symbol == 'X')
                Symbol = FailedSymbol;
        }
    }
}
using System;

namespace Organizer.TaskSystem {
    [Serializable]
    public abstract class CharEnum {
        public char Symbol { get; protected set; }

        protected CharEnum(char symbol) {
            Symbol = symbol;
        }
    }
}
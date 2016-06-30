namespace Organizer.TaskSystem {
    public interface IPortable {
        void Port(ProgramVersion oldVersion);
    }
}
namespace Contracts
{
    public interface IAxis
    {
        bool Move(int axis, double position);

        bool Move(int axis, double xPosition, double yPosition, double zPosition, double rPosition);
    }
}
using iSynaptic.Commons;

namespace ThomsonReuters.CodeGeneration
{
    public interface IMessageMediator<out TObservation>
    {
        IOutcome<TObservation> Publish(object message);
        IResult<T, TObservation> Dispatch<T>(object message);
    }
}
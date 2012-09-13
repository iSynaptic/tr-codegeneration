using System.Collections.Generic;
using System.Linq;

namespace ThomsonReuters.CodeGeneration
{
    public interface IAnswer
    {
        bool WasSuccessful { get; }
        IEnumerable<object> Observations { get; }
    }

    public struct Answer<TObservation> : IAnswer
    {
        public static readonly Answer<TObservation> Success = new Answer<TObservation>(true);
        public static readonly Answer<TObservation> Failure = new Answer<TObservation>(false);

        private readonly bool _IsFailure;
        private readonly TObservation[] _Observations;

        public Answer(bool isSuccess) 
            : this(isSuccess, null)
        {
        }

        public Answer(bool isSuccess, TObservation observation)
            : this()
        {
            _IsFailure = !isSuccess;
            _Observations = new [] { observation };
        }

        public Answer(bool isSuccess, IEnumerable<TObservation> observations)
            : this()
        {
            _IsFailure = !isSuccess;
            _Observations = observations != null
                               ? observations.ToArray()
                               : null;
        }

        IEnumerable<object> IAnswer.Observations 
        { 
            get
            {
                return Observations.Cast<object>();
            }
        }
        bool IAnswer.WasSuccessful 
        { 
            get { return WasSuccessful; }
        }

        public IEnumerable<TObservation> Observations
        {
            get
            {
                if (_Observations != null)
                {
                    foreach (var observation in _Observations)
                        yield return observation;
                }
            }
        }

        public bool WasSuccessful { get { return !_IsFailure; } }

        public static Answer<TObservation> operator&(Answer<TObservation> left, Answer<TObservation> right)
        {
            var l = left;
            var r = right;

            return new Answer<TObservation>(l.WasSuccessful && r.WasSuccessful, l.Observations.Concat(r.Observations));
        }

        public static implicit operator bool(Answer<TObservation> answer)
        {
            return answer.WasSuccessful;
        }

        public static implicit operator Answer<TObservation>(Possible<Nil, TObservation> possible)
        {
            return new Answer<TObservation>(possible.WasSuccessful, possible.Observations);
        }
    }
}

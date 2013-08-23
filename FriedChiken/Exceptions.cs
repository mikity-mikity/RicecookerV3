using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace mikity.Exceptions
{
    public class GeneralException : System.ArgumentException
    {
        public GeneralException(System.String message) : base(message) { }
        public GeneralException() : base("Something went wrong.") { }
    }
    public class NotInitializedException : System.ArgumentException
    {
        public NotInitializedException(System.String message) : base(message) { }
        public NotInitializedException() : base("This class must be initialized first before a specified method is called.") { }
    }
    public class DimensionOfEucledianSpaceNotDetermined : System.ArgumentException
    {
        public DimensionOfEucledianSpaceNotDetermined(System.String message) : base(message) { }
        public DimensionOfEucledianSpaceNotDetermined() : base("Dimension of embeded Eucledian Space is not determined.") { }
    }
    public class NumberOfNodesIncompatibleException : System.ArgumentException
    {
        public NumberOfNodesIncompatibleException(System.String message) : base(message) { }
        public NumberOfNodesIncompatibleException() : base("Number of nodes is incompatible for the specified element.") { }
    }
    public class ValueOutOfRangeException : System.ArgumentException
    {
        public ValueOutOfRangeException(System.String message) : base(message) { }
        public ValueOutOfRangeException() : base("Input values are restricted to some range.") { }
    }
    public class SizeOfMatricesNotCompatibleException : System.ArgumentException
    {
        public SizeOfMatricesNotCompatibleException(System.String message) : base(message) { }
        public SizeOfMatricesNotCompatibleException() : base("Size of matrices must have the same numbers.") { }
    }
    public class SizeOfVectorsNotCompatibleException : System.ArgumentException
    {
        public SizeOfVectorsNotCompatibleException(System.String message) : base(message) { }
        public SizeOfVectorsNotCompatibleException() : base("Size of vectors must have the same numbers.") { }
    }
}

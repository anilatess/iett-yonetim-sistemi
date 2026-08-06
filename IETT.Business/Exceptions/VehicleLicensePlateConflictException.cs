namespace IETT.Business.Exceptions
{
    public class VehicleLicensePlateConflictException : Exception
    {
        public VehicleLicensePlateConflictException()
            : base("Bu plakaya sahip bir araç zaten mevcut.") { }
    }
}

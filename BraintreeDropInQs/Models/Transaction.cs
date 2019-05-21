using GoogleGson.Annotations;

namespace NaxamDemoCopy.Models
{
    public class Transaction: Java.Lang.Object
    {
        [SerializedName(Value = "message")]
        public string Message { get; set; }
    }
}
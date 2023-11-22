using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class NetworkUtils
{
    public static Vector3 ConvertVector3( Protocol.Vector3 vector3 )
    {
        return new Vector3(vector3.X, vector3.Y, vector3.Z);
    }

    public static Protocol.Vector3 ConvertVector3( Vector3 vector3 )
    {
        Protocol.Vector3 res = new()
        {
            X = vector3.x,
            Y = vector3.y,
            Z = vector3.z
        };

        return res;
    }

    public static Quaternion ConvertQuaternion( Protocol.Quaternion vector3 )
    {
        return new Quaternion(vector3.X, vector3.Y, vector3.Z, vector3.W);
    }

    public static Protocol.Quaternion ConvertQuaternion( Quaternion quaternion )
    {
        Protocol.Quaternion res = new()
        {
            X = quaternion.x,
            Y = quaternion.y,
            Z = quaternion.z,
            W = quaternion.w
        };

        return res;
    }
}

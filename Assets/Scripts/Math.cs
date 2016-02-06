using UnityEngine;

public static class Math
{
	public static int Pow(int b, int p)
	{
		int r = 1;
		while (p > 0)
		{
			if ((p & 1) != 0)
				r *= b;
			p >>= 1;
			b *= b;
		}
		return r;
	}

	public static Vector3 UnitVectorFromUV(float u, float v)
	{
		float theta = 2.0f * Mathf.PI * u;
		float phi = Mathf.PI * v;
		return new Vector3(Mathf.Cos(theta) * Mathf.Sin(phi), Mathf.Cos(phi), Mathf.Sin(theta) * Mathf.Sin(phi));
	}

	public static void UVFromUnitVector(Vector3 p, out float u, out float v)
	{
		p.Normalize();
		float theta = Mathf.Atan2(p.z, p.x);
		if (theta < 0.0f)
			theta += 2.0f * Mathf.PI;
		float phi = Mathf.Acos(p.y);
		u = theta / 2.0f / Mathf.PI;
		v = phi / Mathf.PI;
	}
}
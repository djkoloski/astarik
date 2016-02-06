using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;

[TestFixture]
public class MathUnitTests
{
	[Test]
	public void SphericalConversion()
	{
		for (int i = 0; i < 1000; ++i)
		{
			float u = Random.value;
			float v = Random.value;
			Vector3 unit = Math.UnitVectorFromUV(u, v);
			float nu = 0.0f;
			float nv = 0.0f;
			Math.UVFromUnitVector(unit, out nu, out nv);

			Assert.IsTrue(System.Math.Round(u, 4) == System.Math.Round(nu, 4));
			Assert.IsTrue(System.Math.Round(v, 4) == System.Math.Round(nv, 4));
		}
	}
}
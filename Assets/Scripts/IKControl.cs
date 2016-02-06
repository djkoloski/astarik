using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class IKJoint
{
	public Transform jointGameObject;
	// Wraps, maps from 0 - 360
	public float minRotation;
	public float maxRotation;
	// Doesn't wrap, maps from 0 - 180
	public float minBend;
	public float maxBend;

	[HideInInspector]
	[System.NonSerialized]
	public float length;
	[HideInInspector]
	[System.NonSerialized]
	public Quaternion sourceRotation;
	[HideInInspector]
	[System.NonSerialized]
	public Vector3 sourceDirection;

	public Vector3 position
	{
		get
		{
			return jointGameObject.position;
		}
		set
		{
			jointGameObject.position = value;
		}
	}
	public Quaternion rotation
	{
		get
		{
			return jointGameObject.rotation;
		}
		set
		{
			jointGameObject.rotation = value;
		}
	}
}

[System.Serializable]
public class IKNodeData
{
	public Vector3 endEffector;

	public IKNodeData(Vector3 newEndEffector)
	{
		endEffector = newEndEffector;
	}
}

// Basic IKControl, two joints in 2D
public class IKControl :
	MonoBehaviour
{
	public float goalTolerance;
	public int subdivisions;
	public Transform targetObject;
	public IKJoint[] joints;
	public List<float[]> aStarPath;

	private Pathfinding.IHDAStar aStar_;

	public Vector3 Target
	{
		get { return targetObject.position; }
	}
	public int ArmCount
	{
		get { return joints.Length - 1; }
	}

	public float GetArmLength(int index)
	{
		return (joints[index + 1].position - joints[index].position).magnitude;
	}
	public Quaternion GetArmSourceRotation(int index)
	{
		return joints[index].rotation;
	}
	public Vector3 GetArmSourceDirection(int index)
	{
		return (joints[index + 1].position - joints[index].position).normalized;
	}
	public void GetArmValues(int index, out float u, out float v)
	{
		Vector3 along = joints[index + 1].position - joints[index].position;
		Math.UVFromUnitVector(along.normalized, out u, out v);
	}
	public Vector3 CalculateJointPosition(float[] values, int index)
	{
		if (index <= 0)
			return joints[0].position;
		else
		{
			float rotation = values[(index - 1) * 2];
			float bend = values[(index - 1) * 2 + 1];
			return CalculateJointPosition(values, index - 1) + Math.UnitVectorFromUV(rotation, bend) * joints[index - 1].length;
		}
	}
	public Quaternion CalculateJointOrientation(float[] values, int index)
	{
		return Quaternion.FromToRotation(joints[index].sourceDirection, Math.UnitVectorFromUV(values[index * 2], values[index * 2 + 1])) * joints[index].sourceRotation;
	}
	private object CalculateData(float[] values)
	{
		return new IKNodeData(CalculateJointPosition(values, ArmCount));
	}
	private float CalculateCostBetween(object from, float[] fromValues, object to, float[] toValues)
	{
		return (((IKNodeData)to).endEffector - ((IKNodeData)from).endEffector).magnitude;
	}
	private float CalculateHeuristic(object data, float[] values)
	{
		return (Target - ((IKNodeData)data).endEffector).magnitude;
	}
	private bool IsGoal(object data, float[] values)
	{
		return CalculateHeuristic(data, values) <= goalTolerance;
	}
	private bool IsTraversible(object data, float[] values)
	{
		for (int i = 0; i < ArmCount; ++i)
		{
			float rotation = values[2 * i] * 360.0f;
			float bend = values[2 * i + 1] * 180.0f;

			if (rotation < joints[i].minRotation || rotation > joints[i].maxRotation || bend < joints[i].minBend || bend > joints[i].maxBend)
				return false;
		}
		return true;
	}
	private void DrawValues(float[] values, Color color)
	{
		Vector3 lastPosition = CalculateJointPosition(values, 0);
		for (int i = 1; i < joints.Length; ++i)
		{
			Vector3 nextPosition = CalculateJointPosition(values, i);
			Debug.DrawLine(lastPosition, nextPosition, color);
			lastPosition = nextPosition;
		}
	}
	private void SetJointPositions(float[] values)
	{
		for (int i = 0; i < joints.Length; ++i)
		{
			joints[i].position = CalculateJointPosition(values, i);
			if (i != joints.Length - 1)
				joints[i].rotation = CalculateJointOrientation(values, i);
		}
	}
	private void DrawPositions(Color color)
	{
		Vector3 lastPosition = joints[0].position;
		for (int i = 1; i < joints.Length; ++i)
		{
			Debug.DrawLine(lastPosition, joints[i].position, color);
			lastPosition = joints[i].position;
		}
	}

	public void BeginNewSearch()
	{
		bool[] dimensionWrappings = new bool[2 * ArmCount];
		float[] startingValues = new float[2 * ArmCount];

		for (int i = 0; i < ArmCount; ++i)
		{
			dimensionWrappings[2 * i] = true;
			dimensionWrappings[2 * i + 1] = false;
			GetArmValues(i, out startingValues[2 * i], out startingValues[2 * i + 1]);
		}

		aStar_ = new Pathfinding.IHDAStar(2 * ArmCount, subdivisions, dimensionWrappings, CalculateData, CalculateCostBetween, CalculateHeuristic, IsTraversible, IsGoal);
		aStar_.BeginSearch(startingValues);
	}

	public void Start()
	{
		for (int i = 0; i < ArmCount; ++i)
		{
			joints[i].length = GetArmLength(i);
			joints[i].sourceRotation = GetArmSourceRotation(i);
			joints[i].sourceDirection = GetArmSourceDirection(i);
		}

		BeginNewSearch();
	}

	public void Update()
	{
		if (aStar_.Searching)
		{
			for (int i = 0; i < 1000; ++i)
				aStar_.StepSearch();
		}

		if (!aStar_.Searching)
		{
			aStarPath = aStar_.Path;

			if (aStarPath != null)
				SetJointPositions(aStarPath[aStarPath.Count - 1]);

			BeginNewSearch();
		}

		DrawPositions(Color.white);
	}
}
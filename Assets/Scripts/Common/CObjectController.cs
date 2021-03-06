﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CObjectController : MonoBehaviour {

	#region Fields 

	protected Transform m_Transform;
	protected Vector3 m_StartPosition;

	#endregion

	#region Implemetation Monobehaviour

	public virtual void Init() {
	
	}

	protected virtual void Awake() {
		this.m_Transform = this.transform;
		this.m_StartPosition = this.m_Transform.position;
	}

	protected virtual void Start() {
	
	}

	protected virtual void Update() {
		
	}

	protected virtual void LateUpdate() {
		
	}

	protected virtual void OnDrawGizmos() {

	}

	#endregion

	#region Utility

	public virtual bool IsNearestPosition(Vector3 value) {
		var position = this.GetPosition ();
		var direction = position - value;
		var nearestDistance = 0.1f;
		return direction.sqrMagnitude <= nearestDistance;
	}

	#endregion

	#region Getter && Setter


	public virtual void SetPosition(Vector3 position) {
		this.m_Transform.position = position;
	}

	public virtual Vector3 GetPosition() {
		return this.m_Transform.position;
	}

	#endregion

}

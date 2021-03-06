﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace FSM
{
    public enum EState
    {
        StartState = 0,
        UpdateState = 1,
        EndState = 2
    }

    public class FSMManager
	{
		public EState currentState;
		public string currentStateName;
		public string firstStateName;
		public Action<string> OnStartState;
		public Action<string> OnUpdateState;
		public Action<string> OnEndState;

        private Dictionary<string, Func<bool>> m_Conditions;
        private Dictionary<string, IState> m_States;
        private FSMReader m_FSMLoader;
        private FSMStateData m_Map;
        private FSMStateData m_AnyState;
        private FSMStateData m_LastState;
        private EState m_CurrentState;
		private bool m_Inited;

        public FSMManager()
        {
            m_Conditions = new Dictionary<string, Func<bool>>();
            m_States = new Dictionary<string, IState>();
            m_FSMLoader = new FSMReader();
			m_Inited = false;
        }

        public void LoadFSM(string jsonText) {
			m_FSMLoader.LoadJSON(jsonText);
            m_Map = m_FSMLoader.FSMRootStates;
            m_AnyState = m_FSMLoader.FSMAnyStates;
            m_Conditions["IsRoot"] = IsRoot;
            m_Conditions["IsAnyState"] = IsAnyState;
			m_Inited = true;
			firstStateName = m_FSMLoader.FSMRootStates.StateName;
        }

        private bool IsRoot()
        {
            return true;
        }

        private bool IsAnyState() {
            return true;
        }

        public void UpdateState(float dt)
        {
			if (m_Inited == false)
				return;
			for (int i = 0; i < m_AnyState.ListStates.Count; i++)
			{
				var stateNext = m_AnyState.ListStates[i];
				if (stateNext.StateName.Equals(m_Map.StateName))
				{
					continue;
				}
				else if (CalculateCondition (stateNext))
				{
					m_Map = m_AnyState.ListStates[i];
					m_CurrentState = EState.StartState;
					break;
				}
			}
            var stateNow = m_States[m_Map.StateName];
			currentStateName = m_Map.StateName;
            switch (m_CurrentState)
            {
			case EState.StartState:
				stateNow.StartState ();
				m_CurrentState = EState.UpdateState;
				if (OnStartState != null) {
					OnStartState (currentStateName);
				}
                break;
			case EState.UpdateState:
				CalculateStateCondition ();
				if (m_CurrentState == EState.UpdateState) {
					stateNow.UpdateState (dt);
					if (OnUpdateState != null) {
						OnUpdateState (currentStateName);
					}
				}
	            break;
			case EState.EndState:
				if (OnEndState != null) {
					OnEndState (currentStateName);
				}
                stateNow.ExitState();
                m_Map = m_LastState;
                m_CurrentState = EState.StartState;
                break;
            }
        }

        public void RegisterState(string name, IState state)
        {
            if (!m_States.ContainsKey(name))
            {
                m_States[name] = state;
            }
        }

        public void RegisterCondition(string name, Func<bool> condition)
        {
            if (!m_Conditions.ContainsKey(name))
            {
                m_Conditions[name] = condition;
            }
        }

		public void SetState(string name) {
			if (m_Inited == false)
				return;
			m_Map = m_FSMLoader.FSMMaps [name];
			m_CurrentState = EState.StartState;
		}

		private void CalculateStateCondition() {
			for (int i = 0; i < m_Map.ListStates.Count; i++)
			{
				var stateNext = m_Map.ListStates[i];
				if (CalculateCondition(stateNext))
				{
					m_CurrentState = EState.EndState;
					m_LastState = m_Map.ListStates[i];
					break;
				}
			}
		}

		private bool CalculateCondition(FSMStateData state) {
			var condition = state.Condition;
			var result = false;
			for (int i = 0; i < condition.conditionNames.Count; i++) {
				var statement = false;
				if (condition.conditionPrefixes [i] == FSMCondition.EConditionPrefix.Negative) {
					statement = !m_Conditions [condition.conditionNames [i]] ();
				} else {
					statement = m_Conditions [condition.conditionNames [i]] ();
				}
				if (condition.conditionOperators [i] == FSMCondition.EConditionOperator.And) {
					result &= statement;
				} else if (condition.conditionOperators [i] == FSMCondition.EConditionOperator.Or) {
					result |= statement;
				} else {
					result = statement;
				}
			}
			return result;
		}

    }
}

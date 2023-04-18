using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    [CreateAssetMenu(menuName = "Zeus/Table/Quest Table/Quest Data")]
    [System.Serializable]
    public class QuestSO : ScriptableObject
    {
        public int ID;
        public int NameID;
        public int DescriptionID;

        public TypeQuest QuestType;
        public int PrevQuestID = -1;
        public int NextQuestID = -1;
        [Tooltip("해당 ID의 퀘스트와 생존주기를 공유함")]
        public int DependenceQuestID = -1;
        public TypeQuestAccept AcceptType;
        public TypeQuestClear ClearType;
        public int ClearActorID = -1;
        public QuestStepData[] Steps;
        public TypeQuestStepCondition ClearCondition;
        public TypeItem RewardItemType;
        public int RewardItemID;
    }

    public enum TypeQuest
    {
        [InspectorName("메인")]
        MAIN,
        [InspectorName("사이드")]
        SIDE
    }

    public enum TypeQuestAccept
    {
        [InspectorName("자동")]
        AUTO,
        [InspectorName("선택")]
        SELECT,
    }

    public enum TypeQuestClear
    {
        [InspectorName("자동")]
        AUTO,
        [InspectorName("선택")]
        SELECT,
    }

    [System.Serializable]
    public class QuestStepData
    {
        public TypeQuestStep StepType;
        public int TargetID;
        public int StepTargetValue;
    }
    public enum TypeQuestStep
    {
        [InspectorName("대상 접촉")]
        CONTACT_TARGET,
        [InspectorName("대상 처치")]
        KILL_TARGET,
        [InspectorName("지역 입장")]
        VISITE_AREA,
    }
    public enum TypeQuestStepCondition
    {
        AND,
        OR,
    }
}
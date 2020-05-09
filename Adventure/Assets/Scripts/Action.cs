using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action
{
    protected NoSubjectMsg noSubjectMsg;
    protected bool carryOverVerb;
    protected bool subjectOptional;

    public NoSubjectMsg NoSubjectMessage { get { return noSubjectMsg; } }
    public bool CarryOverVerb { get { return carryOverVerb; } }
    public bool SubjectOptional { get { return subjectOptional; } }

    public abstract CommandOutcome DoAction();
    public abstract string FindSubstituteSubject();
}

public struct NoSubjectMsg
{
    public string messageID;
    public string[] messageParams;

    public NoSubjectMsg(string p1, string[] p2)
    {
        messageID = p1;
        messageParams = p2;
    }
}
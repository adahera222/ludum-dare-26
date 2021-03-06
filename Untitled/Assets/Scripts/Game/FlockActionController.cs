using UnityEngine;
using System.Collections;

//collider is used for following, assume it's trigger
public class FlockActionController : ActionListener {
	public FlockUnit flockUnit;
	
	public float actionMaxRadius = 6.75f;
	public float actionCancelRadius = 12.0f; //range to move back to leader
	public float actionCancelDelay = 1.0f; //range to cancel attack if too far from leader
	
	public float followStopDelay = 1.0f;
	public float followStopRadius = 2.0f;
	public float followStopSpeed = 0.01f;
	
	public bool attackStartEnable = false;
	[SerializeField] AttackSensor attackSensor;
	
	[System.NonSerializedAttribute] public Transform leader = null;
	
	private MotionBase mTargetMotion = null;
	
	public bool autoAttack {
		get { return attackSensor != null ? attackSensor.gameObject.activeSelf : false; }
		set { 
			if(attackSensor != null) {
				bool isActive = attackSensor.gameObject.activeSelf;
				if(isActive != value) {
					if(!value) {
						//clear out attack target
						if(currentTarget != null && type == ActionType.Attack) {
							StopAction(ActionTarget.Priority.Highest, true);
						}
					}
					
					attackSensor.gameObject.SetActive(value);
				}
			}
		}
	}
	
	// Use this to determine if we can attack in range
	public override bool CheckRange() {
		if(currentTarget != null) {
			switch(type) {
			case ActionType.Attack:
				if(flockUnit != null) {
					return attackSensor != null && flockUnit.enabled && attackSensor.CheckRange(flockUnit.dir, currentTarget.transform);
				}
				else {
					return true;
				}
				
			default:
				break;
			}
		}
		
		return true;
	}
		
	public override void SetActive(bool activate) {
		base.SetActive(activate);
		
		if(!activate) {
			ResetAutoActions();
		}
	}
	
	protected override void OnActionEnter() {
		mTargetMotion = currentTarget.GetComponent<MotionBase>();
		
		switch(type) {
		case ActionType.Disperse:
			break;
			
		case ActionType.Attack:
			if(flockUnit != null) {
				flockUnit.moveTarget = currentTarget.transform;
			}
						
			if(gameObject.activeInHierarchy)
				StartCoroutine("ReturnToLeader");
			
			//no need to constantly check
			if(attackSensor != null) {
				attackSensor.enabled = false;
				
				if(flockUnit != null) {
					flockUnit.minMoveTargetDistance = attackSensor.minRange;
				}
			}
			break;
			
		case ActionType.Retreat:
		case ActionType.Follow:
			if(flockUnit != null) {
				flockUnit.moveTarget = currentTarget.target;
			}
			
			if(gameObject.activeInHierarchy)
				StartCoroutine("FollowStop");
			break;
			
		default:
			if(flockUnit != null) {
				flockUnit.moveTarget = currentTarget.target;
			}
			break;
		}
	}
	
	protected override void OnActionExit() {
	}
	
	protected override void OnActionFinish() {
		if(flockUnit != null) {
			flockUnit.Stop();
		}
		
		mTargetMotion = null;
		
		if(attackSensor != null) { //renabled after attack
			attackSensor.enabled = true;
		}
		
		StopCoroutine("FollowStop");
		StopCoroutine("ReturnToLeader");

        //if no default, see if we have a waypoint to follow
	}
	
	protected override void OnDestroy ()
	{
		if(attackSensor != null) {
			attackSensor.stayCallback -= AutoAttackCheck;
		}
		
		base.OnDestroy ();
	}
	
	protected virtual void Awake() {
		ResetAutoActions();
		
		if(attackSensor != null) {
			attackSensor.stayCallback += AutoAttackCheck;
		}
		
	}

    protected virtual void AutoAttackCheck(UnitBaseEntity unit) {
		if(!(type == ActionType.Attack || type == ActionType.Retreat)) {
			ActionTarget target = unit.actionTarget;
			if(target != null && target.type == ActionType.Attack && target.vacancy && currentPriority <= target.priority) {
				currentTarget = target;
			}
		}
	}
	
	IEnumerator ReturnToLeader() {
		//see if we are too far from leader
		float radiusCancelSqr = actionCancelRadius*actionCancelRadius;
		float radiusMoveBackSqr = actionMaxRadius*actionMaxRadius;
		
		while(!lockAction && currentTarget != null && currentTarget != defaultTarget && leader != null) {
			yield return new WaitForSeconds(actionCancelDelay);
			
			Vector2 pos = transform.position;
			Vector2 leaderPos = leader.position;
			float distSqr = (leaderPos - pos).sqrMagnitude;
			
			if(distSqr > radiusCancelSqr) { //cancel current action
				StopAction(ActionTarget.Priority.High, true);
			}
			else if(flockUnit != null) { //don't cancel action, just move within leader's vicinity
				if(distSqr > radiusMoveBackSqr && !lockAction) {
					flockUnit.moveTarget = leader;
				}
				else {
					flockUnit.moveTarget = currentTarget.transform;
				}
			}
		}
	}
	
	IEnumerator FollowStop() {
		while(currentTarget != null && mTargetMotion != null) {
			yield return new WaitForSeconds(followStopDelay);
			
			switch(type) {
			case ActionType.Retreat:
			case ActionType.Follow:
				if(flockUnit != null) {
					if(mTargetMotion.curSpeed < followStopSpeed) {
						if(flockUnit.moveTarget != null && flockUnit.moveTargetDistance <= followStopRadius)
							flockUnit.moveTarget = null;
					}
					else {
						flockUnit.moveTarget = currentTarget.target;
					}
				}
				
				yield return new WaitForFixedUpdate();
				break;
				
			default:
				yield break;
			}
		}
		
		yield break;
	}
	
	void ResetAutoActions() {
		if(attackSensor != null) {
			attackSensor.gameObject.SetActive(attackStartEnable);
			attackSensor.enabled = true;
		}
		
	}
			
	void OnDrawGizmosSelected() {
		Gizmos.color = new Color(208.0f/255.0f, 149.0f/255.0f, 208.0f/255.0f);
		
		if(actionCancelRadius > 0.0f)
			Gizmos.DrawWireSphere(transform.position, actionCancelRadius);
		
		Gizmos.color *= 0.4f;
		
		if(actionMaxRadius > 0.0f)
			Gizmos.DrawWireSphere(transform.position, actionMaxRadius);
	}
}

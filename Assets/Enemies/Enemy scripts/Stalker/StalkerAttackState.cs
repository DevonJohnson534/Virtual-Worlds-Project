using System.Collections;
using System.Diagnostics.Metrics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityHFSM;
using VirtualWorlds.FSM;


namespace VirtualWorlds.FSM
{
    
    public class StalkerAttackState : StalkerAttackBase
    {
        readonly private InputHandler target;

        readonly private Transform ArmBone;

        readonly private DamageConfigScriptableObject damageConfig;

        private bool isThrowing = false;

        private bool isFinishing = false;

        public delegate void AttackEvent();
        public event AttackEvent SuccessfulAttack;

        private Camera playerCamera;
        private Renderer HeadTransparency;
        private Renderer GlassesTransparency;
        private Renderer GlassTransparency;

        public StalkerAttackState(bool needsExitTime,
            Stalker stalker,
            InputHandler target,
            Transform ArmBone,
            DamageConfigScriptableObject damageConfig, 
            Camera playerCamera,
            Renderer HeadTransparency,
            Renderer GlassesTransparency,
            Renderer GlassTransparency) : base(needsExitTime, stalker)
        {
            this.target = target;
            this.ArmBone = ArmBone;
            this.damageConfig = damageConfig;
            this.playerCamera = playerCamera;
            this.HeadTransparency = HeadTransparency;
            this.GlassesTransparency = GlassesTransparency;
            this.GlassTransparency = GlassTransparency;
        }

        public override void OnEnter()
        {
            // Can handle damage here with the knowledge in THIS video
            // https://www.youtube.com/watch?v=6yvUmSxlGQo
            //agent.isStopped = true;  <-- if we want our guy to stop moving during attack
            //agent.move() is a method you can use to move your guy without it continuing
            //to move at a given destination, rather just move straight forward so it doesn't perfectly
            //track the player.  
            base.OnEnter();
            stalker.attackSensor.GetComponent<SphereCollider>().radius = 40f;
            stalker.grabSensor.SuccessfulAttack += OnTriggerEnter;
            stalker.grabSensor.GetComponent<BoxCollider>().enabled = false;
            agent.isStopped = true;
            isFinishing = false;
            isThrowing = false;
            Debug.Log("Entered Attack state!");
            animator.SetTrigger("attack");
        }

        public override void OnLogic()
        {
            // Note to self : DOUBLE CHECK WHAT THE FUCK A PARENT DOES.
            // SO MUCH OF THIS CODE was written for FUN because of that one simple blunder.
            //The parent originally was StalkerStateBase - which, fun fact, would automatically
            //decide the state was ready to leave of its own accord so long as an exit was requested.
            // Works great for walk cyclkes or instant swaps, but for a beast like this state
            // it's actively detrimental behavior.

            base.OnLogic();

            //This is here to stop OnLogic from doing anything when it shouldn't be (when he's throwing)
            //(and when he's exiting the state.)
            if (isThrowing || isFinishing)
            {
                return;
            }

            //If he's not doing either, a new state is allowed to occur.
            else if (requestedExit)
            {
                fsm.StateCanExit();
            }
            //These are basically lines that tell the stalker to enable its grab box once the animation actually begins.
            //This is to prevent cases where the stalker is in the attack state and prematurely grab people despite
            //having a mismatched animation. Just gives it time to catch up - this is also why it's not enabled on entry,
            //cuz when we're entering we could still theoretically be all the way back in the whiff animation. 
            //That second one, in the or, is there because the attack animation WILL actually be happening, 
            //It's just that internally the animator think's its still the chase animation, because while it is interupptable,
            //unity still interally waits for its timer to tick down before registering a new animation.
            //So, we know implicitly if the next thing is big attack, and we're in this state, then we canceled the chase animation
            //and are properly visiually aligned.
            else if((stalker.grabSensor.GetComponent<BoxCollider>().enabled == false && animator.GetCurrentAnimatorStateInfo(0).IsName("Big_attack"))
                || (animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName("Big_attack")))
            {
                stalker.grabSensor.GetComponent<BoxCollider>().enabled = true;
            }

            //In the event the animation finishes - same deal, y'know, don't want a guy standing completely still to grab ya.
            //We set the radius to .1 as a way to "force" the stalker to register it missed, because it will cause the player to 
            //leave its attack trigger forcefully. We fix this on exit so it's back to proper size.
            else if (stalker.grabSensor.GetComponent<BoxCollider>().enabled == true 
                && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                stalker.grabSensor.GetComponent<BoxCollider>().enabled = false;
                Debug.Log("Attack sensor range reset.");
                stalker.attackSensor.GetComponent<SphereCollider>().radius = .1f;
            }
        }

        public override void OnExit()
        {
            isFinishing = true;
            stalker.grabSensor.GetComponent<BoxCollider>().enabled = false;
            stalker.attackSensor.GetComponent<SphereCollider>().radius = 1.3f;

            // This has to be added, because if you don't call this line on exit,
            // Every time you enter you're resubscribing to the trigger, and therefore,
            // when the trigger happens, it's called several times, one for each resubscription.
            stalker.grabSensor.SuccessfulAttack -= OnTriggerEnter;
            Debug.Log("Attacker's on exit has run successfully!");
        }

        public void OnTriggerEnter(Collider other)
        {
            
            if (other.TryGetComponent(out PlayerController player))
            {
                // Don't want him grabbing several times.
                stalker.grabSensor.GetComponent<BoxCollider>().enabled = false;
                isThrowing = true;
                stalker.StartCoroutine(ThrowSequence(player));
            }
        }

        // This, is a coroutine chain! This is notably more resource efficient then dicking around in a 
        // busy while loop. So basically, how it works, is we have our first coroutine! The return
        // for any coroutine needs to be IEnumerator. This first coroutine utilizes a yield return at
        // The first coroutine - this is to effectively running whenever we're waiting on an animation.

        // This is because the first coroutine will NOT run until the entirety of its coroutine has finished,
        // Which means every yield return we do in that one to wait, is respected and pretty much entirely pauses execution. 
        // OnLogic WILL still run during this, but it's not a CPU at 100% usage for an animations duration so it's well worth the
        // Check. 

        IEnumerator ThrowSequence(PlayerController player)
        {
            Debug.Log("Entering Throw Sequence!");
            yield return stalker.StartCoroutine(Throw(player));
            isThrowing = false;
        }

        IEnumerator Throw(PlayerController player)
        {
            SuccessfulAttack.Invoke();
            animator.SetTrigger("stab");
            // Just stops us the players from knocking him around while he throws.
            stalker.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Quaternion OGRotation = player.transform.rotation;
            target.enabled = false;

            // Because the pill is big, for the POV of the stab to work, we need him to be smaller. it effects nothing gamplay
            // wise cuz it only happens in this cutscene.
            player.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

            // ALlow me to digest this nonsense for you, future me! Basically, we're grabbing the Armbone's vertical position pushing it down slightly.
            // Then, We're taking the direction  of the Stalker's Z axis, and we're pushing ourself forwards slightly in that direction. 
            // Finally, we're taking the stalker's x axis and pushing ourselves opposite of it.

            //All of this just to align an animation, yeeesh. These took me FIVE HOURS just find the right numbers, but a lot of that was also learning how it worked.
            //Doing a loop with these IS incremental, just very subtle without any specific interpolation. 

            //Also interpolation is a WAAAYYY better way of doing it than just raw numbers please do that in the future instead of this mess. 
            player.transform.position = ArmBone.transform.position + Vector3.down * 0.1f + stalker.transform.forward * 0.1f + stalker.transform.right * -0.3f;

            // This tilts the pill forwards slightly on the x axis, so it makes it lean down. 
            player.transform.rotation = stalker.transform.rotation * Quaternion.Euler(10f, 0f, 0f);

            // Custom method that resets the rotation of the camera to ensure the animation doesn't clip due to initial camer angles.
            player.ResetRotation();
            // This sets the camera's "start" closer to the character model to allow for that desired POV, ordinarily it'd clip
            // if it were this close but due to the reduced scale it can happen safely. 
            playerCamera.nearClipPlane = 0.1f;

            float easing_angle = 90;
            float easing_z = 0.4f;
            float easing_y = 0.05f;

            // The head was king clipper so I decided WHILE YOU'RE THROWING YOU DON'T GET ONE, STALKER.
            // Then he said "bro why" and I said "BECAUSE I'M SICK OF TWEAKING NUMBERS, NOW DIE!"

            // ... that didn't actually happen, believe it or not, I'm just a good story teller. anyway
            // These are the mesh renderers, by disabling them you essentially make the mesh invisible.

            HeadTransparency.enabled = false;
            GlassesTransparency.enabled = false;
            GlassTransparency.enabled = false;

            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Big_attack_stab")) {
                // Same stuff as above but with INTERPOLATION! Basically, Lerp (and its brother, LerpAngle, which is just Lerp but it loops back to 360 if it ever goes negative... so...
                // It's for angles. Anyway, they both will smoothly transition between the current number, in this case, the "easing" variables, and the second provided argument, which is the
                // desired endpoint. They do this independent of framerate, too! That third one's a bit fucky, but the only number you need give a damn about is the -4, -2, and -4 in the parenthesis.
                // Basically the bigger the absolute value of the number, the faster the transition between the two numbers. 

                // here it's going pretty fast. 0.1 is slow, 1 is decently fast., 2 is fast, and 4 is fuckin' bookin' it. It's important to remember that these transitions start FAST, then taper
                // off as they conclude, so if you need it fast the whole way through numbers like 2 and 4 are necessary. 
                // That top part just updates the positions according to what our "smooth" value is. We use the stalker and armbone as relational benchmarks for ease of adjustment. 
                player.transform.position = ArmBone.transform.position + Vector3.down * easing_y + stalker.transform.forward * easing_z + stalker.transform.right * -0.3f;
                player.transform.rotation = stalker.transform.rotation * Quaternion.Euler(easing_angle, 0f, 0f);
                easing_angle = Mathf.LerpAngle(easing_angle, 30, 1f - Mathf.Exp(-4 * Time.deltaTime));
                easing_z = Mathf.Lerp(easing_z, -0.3f, 1 - Mathf.Exp(-2f * Time.deltaTime));
                easing_y = Mathf.Lerp(easing_y, -0.1f, 1 - Mathf.Exp(-4f * Time.deltaTime));
                yield return null; // Wait for the next frame
            }

            if (player.TryGetComponent(out Damageable damageable))
            {
                // Super micro adjustment, not even really sure if it does anything. But also, damage! Neat. 
                player.transform.position = ArmBone.transform.position + Vector3.down * -0.1f + stalker.transform.forward * -0.2f + stalker.transform.right * -0.3f;
                player.transform.rotation = stalker.transform.rotation * Quaternion.Euler(30f, 0f, 0f);
                damageable.TakeDamage(damageConfig.GetDamage(5));
            }
            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.803f) {
                //These adjust with constant values. Goof for micro movements like breathing, unideal for more intensive movement. 
                player.transform.position =ArmBone.transform.position + Vector3.down * -0.12f + stalker.transform.forward * 0.09f + stalker.transform.right * -0.3f;
                player.transform.rotation = stalker.transform.rotation * Quaternion.Euler(29f, 0f, 0f);
                yield return null; 
            }
            // We say we can exit here so that the Stalker knows to go to idle before the player leaves the attack bubble, which would proc a the whiff transition. 
            fsm.StateCanExit();
            HeadTransparency.enabled = true;
            GlassesTransparency.enabled = true;
            GlassTransparency.enabled = true;
            playerCamera.nearClipPlane = 0.3f;
            player.transform.localScale = new Vector3(1, 1, 1);



            // So this some BULLSHIT, but to summerize, it basically is a smooth way to make the player look at the stalker, then falllll
            // back as they're THROWN away for some nice cinnematic flair. 
            float current_x = player.transform.rotation.x;
            float current_y = player.transform.rotation.y;
            float target_x = -90f;
            float target_y = -180f;
            int counter = 0;
            float speed_multiplier = 3f;
            float speed = -2f;
            while (counter < 300)
            {
                player.transform.rotation = stalker.transform.rotation * Quaternion.Euler(current_x, current_y, 0f);
                current_y = Mathf.LerpAngle(current_y, target_y, 1f - Mathf.Exp(-70 * Time.deltaTime));
                current_x = Mathf.LerpAngle(current_x, target_x, 1f - Mathf.Exp(-1f * Time.deltaTime));

                player.Move(new Vector2(0, speed_multiplier * speed), true);
                yield return new WaitForSeconds(0.01f);
                speed += 0.006f;
                counter++;
            }
            stalker.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            // Because of the fucked up way unity's character controller works, we need to smoothly return to the player's rotation 
            // pre-throw in order to prevent camera jolting nonsense. It also give the effect of standing up! 
            current_x = player.transform.rotation.x;
            target_x = 0f;
            counter = 0;
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, OGRotation, 0.2f * Time.deltaTime);
            while(counter < 80)
            {
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, OGRotation, 24f * Time.deltaTime);
                counter++; 
                yield return new WaitForSeconds(0.01f);
            }
            target.enabled = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TST_PlagueGun
{
    public class ModExtension_PlagueBullet : DefModExtension
    {
        public float addHediffChance = 0.5f;
        public HediffDef hediffToAdd;
    }
    public class Projectile_PlagueBullet : Bullet
    {
        // This is a property, which is essentially a method without any arguments.
        // The => notation is a simplified way to give it a getter without any setter.
        // We're essentially aliasing base.def.GetModExtension<ModExtension_PlagueBullet>(), which is quite a mouthful, with a much shorter term.
        public ModExtension_PlagueBullet Props => base.def.GetModExtension<ModExtension_PlagueBullet>();

        #region Overrides
        protected override void Impact(Thing hitThing)
        {
            /* This is a call to the Impact method of the class we're inheriting from.
             * You can use your favourite decompiler to see what it does, but suffice to say
             * there are useful things in there, like damaging/killing the hitThing.
             */
            base.Impact(hitThing);

            /*
             * Null checking is very important in RimWorld.
             * 99% of errors reported are from NullReferenceExceptions (NREs).
             * Make sure your code checks if things actually exist, before they
             * try to use the code that belongs to said things.
             */
            if (Props != null && hitThing != null && hitThing is Pawn hitPawn) //Fancy way to declare a variable inside an if statement. - Thanks Erdelf.
            {
                float rand = Rand.Value; // This is a random percentage between 0% and 100%
                if (rand <= Props.addHediffChance) // If the percentage falls under the chance, success!
                {
                    /*
                     * Messages.Message flashes a message on the top of the screen. 
                     * You may be familiar with this one when a colonist dies, because
                     * it makes a negative sound and mentioneds "So and so has died of _____".
                     * 
                     * Here, we're using the "Translate" function. More on that later in
                     * the localization section.
                     */
                    Messages.Message("TST_PlagueBullet_SuccessMessage".Translate(
                        this.launcher.Label, hitPawn.Label
                    ), MessageTypeDefOf.NeutralEvent);

                    // This checks to see if the character has a health differential, or hediff, on them already.
                    Hediff plagueOnPawn = hitPawn.health?.hediffSet?.GetFirstHediffOfDef(Props.hediffToAdd);

                    // This is an example of hardcoding a variable, which is generally bad practice. I've kept it from the original code as an example.
                    // A good exercise to see whether you understand exposing variables to XML would be to add a FloatRange to the ModExtension and get a random value from it here.
                    float randomSeverity = Rand.Range(0.15f, 0.30f);
                    if (plagueOnPawn != null)
                    {
                        // If they already have plague, add a random range to its severity.
                        // If severity reaches 1.0f, or 100%, plague kills the target.
                        plagueOnPawn.Severity += randomSeverity;
                    }
                    else
                    {
                        // These three lines create a new health differential or Hediff,
                        // put them on the character, and increase its severity by a random amount.
                        Hediff hediff = HediffMaker.MakeHediff(Props.hediffToAdd, hitPawn);
                        hediff.Severity = randomSeverity;
                        hitPawn.health.AddHediff(hediff);
                    }
                }
                else // failure!
                {
                    /*
                     * Motes handle all the smaller visual effects in RimWorld.
                     * Dust plumes, symbol bubbles, and text messages floating next to characters.
                     * This mote makes a small text message next to the character.
                     */
                    MoteMaker.ThrowText(hitThing.PositionHeld.ToVector3(), hitThing.MapHeld, "TST_PlagueBullet_FailureMote".Translate(Props.addHediffChance), 12f);
                }
            }
        }
        #endregion Overrides
    }
}

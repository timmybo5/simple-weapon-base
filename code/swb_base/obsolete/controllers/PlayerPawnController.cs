using System;
using System.Collections.Generic;
using Sandbox;

/* Result from Pain Day 4, this will be here temporarily until it is clear how templates work */

namespace SWB_Base
{
    public class PlayerPawnController : BaseNetworkable
    {
        internal HashSet<string> Events;
        internal HashSet<string> Tags;

        public Entity Pawn { get; protected set; }
        public Client Client { get; protected set; }
        public Vector3 Position { get; set; }
        public Rotation Rotation { get; set; }
        public Vector3 Velocity { get; set; }
        public Rotation EyeRotation { get; set; }
        public Vector3 EyeLocalPosition { get; set; }
        public Vector3 BaseVelocity { get; set; }
        public Entity GroundEntity { get; set; }
        public Vector3 GroundNormal { get; set; }

        public Vector3 WishVelocity { get; set; }

        public void UpdateFromEntity(Entity entity)
        {
            Position = entity.Position;
            Rotation = entity.Rotation;
            Velocity = entity.Velocity;

            EyeRotation = entity.EyeRotation;
            EyeLocalPosition = entity.EyeLocalPosition;
            BaseVelocity = entity.BaseVelocity;
            GroundEntity = entity.GroundEntity;
            WishVelocity = entity.Velocity;
        }

        public void UpdateFromController(PlayerPawnController controller)
        {
            Pawn = controller.Pawn;
            Client = controller.Client;

            Position = controller.Position;
            Rotation = controller.Rotation;
            Velocity = controller.Velocity;
            EyeRotation = controller.EyeRotation;
            GroundEntity = controller.GroundEntity;
            BaseVelocity = controller.BaseVelocity;
            EyeLocalPosition = controller.EyeLocalPosition;
            WishVelocity = controller.WishVelocity;
            GroundNormal = controller.GroundNormal;

            Events = controller.Events;
            Tags = controller.Tags;
        }

        public void Finalize(Entity target)
        {
            target.Position = Position;
            target.Velocity = Velocity;
            target.Rotation = Rotation;
            target.GroundEntity = GroundEntity;
            target.BaseVelocity = BaseVelocity;
            target.EyeLocalPosition = EyeLocalPosition;
            target.EyeRotation = EyeRotation;
        }

        /// <summary>
        /// This is what your logic should be going in
        /// </summary>
        public virtual void Simulate()
        {
            // Nothing
        }

        /// <summary>
        /// This is called every frame on the client only
        /// </summary>
        public virtual void FrameSimulate()
        {
            Host.AssertClient();
        }

        /// <summary>
        /// Call OnEvent for each event
        /// </summary>
        public virtual void RunEvents(PlayerPawnController additionalController)
        {
            if (Events == null) return;

            foreach (var e in Events)
            {
                OnEvent(e);
                additionalController?.OnEvent(e);
            }
        }

        /// <summary>
        /// An event has been triggered - maybe handle it
        /// </summary>
        public virtual void OnEvent(string name)
        {

        }

        /// <summary>
        /// Returns true if we have this event
        /// </summary>
        public bool HasEvent(string eventName)
        {
            if (Events == null) return false;
            return Events.Contains(eventName);
        }

        /// <summary>
        /// </summary>
        public bool HasTag(string tagName)
        {
            if (Tags == null) return false;
            return Tags.Contains(tagName);
        }


        /// <summary>
        /// Allows the controller to pass events to other systems
        /// while staying abstracted.
        /// For example, it could pass a "jump" event, which could then
        /// be picked up by the playeranimator to trigger a jump animation,
        /// and picked up by the player to play a jump sound.
        /// </summary>
        public void AddEvent(string eventName)
        {
            // TODO - shall we allow passing data with the event?

            if (Events == null) Events = new HashSet<string>();

            if (Events.Contains(eventName))
                return;

            Events.Add(eventName);
        }


        /// <summary>
        /// </summary>
        public void SetTag(string tagName)
        {
            // TODO - shall we allow passing data with the event?

            Tags ??= new HashSet<string>();

            if (Tags.Contains(tagName))
                return;

            Tags.Add(tagName);
        }

        /// <summary>
        /// Allow the controller to tweak input. Empty by default
        /// </summary>
        public virtual void BuildInput(InputBuilder input)
        {

        }

        public void Simulate(Client client, Entity pawn, PlayerPawnController additional)
        {
            Events?.Clear();
            Tags?.Clear();

            Pawn = pawn;
            Client = client;

            UpdateFromEntity(pawn);

            Simulate();

            additional?.UpdateFromController(this);
            additional?.Simulate();
            RunEvents(additional);

            if (additional != null)
            {
                additional.Finalize(pawn);
            }
            else
            {
                Finalize(pawn);
            }
        }

        public void FrameSimulate(Client client, Entity pawn, PlayerPawnController additional)
        {
            Pawn = pawn;
            Client = client;

            UpdateFromEntity(pawn);

            FrameSimulate();

            additional?.UpdateFromController(this);
            additional?.FrameSimulate();

            if (additional != null)
            {
                additional.Finalize(pawn);
            }
            else
            {
                Finalize(pawn);
            }
        }
    }
}

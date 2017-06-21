using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HouraiTeahouse.SmashBrew.States;

namespace HouraiTeahouse.SmashBrew.Characters {

    [CreateAssetMenu]
    public partial class CharacterControllerBuilder : ScriptableObject, ISerializationCallbackReceiver {

        [Serializable]
        public class StateData {
            public string Name;
            public CharacterStateData Data;
        }

        public StateData[] _data;
        Dictionary<string, CharacterStateData> _dataMap;

        public StateControllerBuilder<CharacterState, CharacterStateContext> Builder { get; set; }

        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        protected CharacterState State(string name, CharacterStateData data) {
            var state = new CharacterState(name, data);
            if (Builder != null)
                Builder.AddState(state);
            return state;
        }

        void InjectState(object obj, string path = "", int depth = 0) {
            var type = typeof(CharacterState);
            foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties(flags)) {
                string propertyName = propertyInfo.Name;
                if (!string.IsNullOrEmpty(path))
                    propertyName = path + "." + propertyName;
                if (propertyName == "name" || propertyName == "hideFlags" || propertyName == "Builder")
                    continue;
                var propertyType = propertyInfo.PropertyType;
                object instance;
                if (propertyType == type) {
                    if (_dataMap == null)
                        _dataMap = new Dictionary<string, CharacterStateData>();
                    if (!_dataMap.ContainsKey(propertyName))
                        _dataMap.Add(propertyName, new CharacterStateData());
                    var state = new CharacterState(propertyName, _dataMap[propertyName]);
                    Builder.AddState(state);
                    instance = state;
                } else {
                    instance = Activator.CreateInstance(propertyType);
                    if (depth < 7)
                        InjectState(instance, propertyName, depth + 1);
                }
                propertyInfo.SetValue(obj, instance, null);
            }
        }

        protected Func<CharacterStateContext, bool> Input(Func<InputContext, bool> input) {
            Argument.NotNull(input);
            return ctx => input(ctx.Input);
        }

        protected Func<CharacterStateContext, bool> Attack(Func<InputContext, bool> inputFunc = null) {
            if (inputFunc == null)
                return ctx => ctx.Input.Attack.WasPressed;
            return ctx => ctx.Input.Attack.WasPressed && inputFunc(ctx.Input);
        }

        protected Func<CharacterStateContext, bool> Special(Func<InputContext, bool> inputFunc = null) {
            if (inputFunc == null)
                return ctx => ctx.Input.Special.WasPressed;
            return ctx => ctx.Input.Special.WasPressed && inputFunc(ctx.Input);
        }

        public StateController<CharacterState, CharacterStateContext> BuildCharacterControllerImpl(StateControllerBuilder<CharacterState, CharacterStateContext> builder) {
            Builder = builder;
            InjectState(this);

            // Declare Smash Attacks
            SmashUp.Charge.Data.SmashAttack = SmashAttack.Charge;
            SmashSide.Charge.Data.SmashAttack = SmashAttack.Charge;
            SmashDown.Charge.Data.SmashAttack = SmashAttack.Charge;

            SmashUp.Attack.Data.SmashAttack = SmashAttack.Attack;
            SmashSide.Attack.Data.SmashAttack = SmashAttack.Attack;
            SmashDown.Attack.Data.SmashAttack = SmashAttack.Attack;

            //TODO(james7132): Make this configurable
            const float inputThreshold = 0.1f;

            // Ground Attacks
            new [] {Idle, Walk, CrouchStart, Crouch, CrouchEnd}
                // Tilt Attacks
                .AddTransitions(TiltUp, Attack(i => i.Movement.y > inputThreshold))
                .AddTransitions(TiltDown, Attack(i => i.Movement.y < -inputThreshold))
                .AddTransitions(TiltSide, Attack(i => Math.Abs(i.Movement.x) > inputThreshold))
                // Smash Attacks
                .AddTransitions(SmashUp.Charge, Attack(i => i.Smash.y > inputThreshold))
                .AddTransitions(SmashSide.Charge, Attack(i => Math.Abs(i.Smash.x) > inputThreshold))
                .AddTransitions(SmashDown.Charge, Attack(i => i.Smash.y < -inputThreshold))
                // Neutral Combo
                .AddTransitions(Neutral, Attack());
            SmashUp.Charge.AddTransitionTo(SmashUp.Attack);
            SmashDown.Charge.AddTransitionTo(SmashDown.Attack);
            SmashSide.Charge.AddTransitionTo(SmashSide.Attack);
            new[] {Neutral, TiltUp, TiltDown, TiltSide, SmashUp.Attack, SmashDown.Attack, SmashSide.Attack}
                .AddTransitionTo(Idle);

            // Aerial Attacks
            new [] {Fall, Jump, JumpAerial}
                .AddTransitions(AerialUp, Attack(i => i.Movement.y > inputThreshold))
                .AddTransitions(AerialDown, Attack(i => i.Movement.y < -inputThreshold))
                // TODO(james7132): Make these face in the right direction
                .AddTransitions(AerialForward, ctx => 
                    Attack(i => i.Movement.x * Mathf.Sign(ctx.Direction) > inputThreshold)(ctx))
                .AddTransitions(AerialBackward, ctx => 
                    Attack(i => i.Movement.x * Mathf.Sign(ctx.Direction) < -inputThreshold)(ctx))
                .AddTransitions(AerialNeutral, Attack());
            new[] {AerialForward, AerialBackward, AerialDown, AerialUp, AerialNeutral}
                .AddTransitions(AerialAttackLand, ctx => ctx.IsGrounded)
                .AddTransitionTo(Fall);
            AerialAttackLand.AddTransitionTo(Idle);

            // Aerial Movement
            new [] {Idle, Walk, Dash, Run, RunTurn, RunBrake, CrouchStart, Crouch, CrouchEnd, Shield.Main} 
                .AddTransitions(JumpStart, Input(i => i.Jump.WasPressed));
            new[] {JumpStart, JumpAerial}.AddTransitionTo(Jump);
            new[] {Jump, Fall}.AddTransitions(JumpAerial, Input(i => i.Jump.WasPressed))
                              .AddTransitions(EscapeAir, Input(i => i.Shield.WasPressed));
            Jump.AddTransition(Idle, ctx => ctx.NormalizedAnimationTime >= 1.0f && ctx.IsGrounded)
                .AddTransition(Fall, ctx => ctx.NormalizedAnimationTime >= 1.0f && !ctx.IsGrounded);
            EscapeAir.AddTransitionTo(FallHelpless);
            new[] {Fall, FallHelpless, EscapeAir}.AddTransitions(Land, ctx => ctx.IsGrounded);
            Land.AddTransitionTo(Idle);

            // Running States
            Idle.AddTransition(Dash, Input(i => Mathf.Abs(i.Smash.x) > inputThreshold));
            Dash.AddTransitionTo(Idle, Input(i => Mathf.Abs(i.Movement.x) < inputThreshold));
            new[] {Dash, RunTurn}.AddTransitionTo(Run);
            Run.AddTransition(RunBrake, Input(i => Mathf.Abs(i.Movement.x) < inputThreshold));
            Run.AddTransition(RunTurn,
                ctx => !Mathf.Approximately(Mathf.Sign(ctx.Input.Movement.x), Mathf.Sign(ctx.Direction)));
            RunBrake.AddTransitionTo(Idle);

            // Ground Movement 
            new[] {Idle, Walk, Run}
                .AddTransitions(CrouchStart, Input(i => i.Movement.y < -inputThreshold))
                .AddTransitions(Fall, ctx => !ctx.IsGrounded);
            Idle.AddTransition(Walk, ctx => Math.Abs(ctx.Input.Movement.x) > inputThreshold);
            Walk.AddTransition(Idle, ctx => Math.Abs(ctx.Input.Movement.x) < inputThreshold);

            // Crouching States
            CrouchStart.AddTransitionTo(Crouch);
            Crouch.AddTransition(CrouchEnd, Input(i => i.Movement.y >= -inputThreshold));
            CrouchEnd.AddTransitionTo(Idle);

            // Ledge States
            new[] {Idle, Fall, FallHelpless}.AddTransitions(LedgeGrab, ctx => ctx.IsGrabbingLedge);
            LedgeGrab.AddTransitionTo(LedgeIdle);
            LedgeIdle.AddTransition(LedgeRelease, Input(i => i.Movement.y < -inputThreshold))
                .AddTransition(LedgeClimb, Input(i => i.Movement.y > inputThreshold))
                .AddTransition(LedgeJump, Input(i => i.Jump.WasPressed))
                .AddTransition(LedgeAttack, Attack());
            LedgeJump.AddTransitionTo(Jump);
            new[] {LedgeRelease, LedgeClimb, LedgeEscape}
                .AddTransitions(Idle, ctx => ctx.NormalizedAnimationTime >= 1.0f && ctx.IsGrounded)
                .AddTransitions(Fall, ctx => ctx.NormalizedAnimationTime >= 1.0f && !ctx.IsGrounded);

            // Shielding
            Idle.AddTransition(Shield.On, Input(i => i.Shield.Current));
            Shield.On.AddTransition(Shield.Perfect, ctx => ctx.IsHit)
                .AddTransitionTo(Shield.Main);
            Shield.Main.AddTransition(Shield.Broken, ctx => ctx.ShieldHP < 0)
                .AddTransition(Shield.Off, Input(i => !i.Shield.Current));
            new[] {Shield.Broken, Shield.Stunned, Idle}.Chain();
            
            // Rolls/Sidesteps
            Shield.Main.AddTransition(EscapeForward, Input(i => i.Movement.x > inputThreshold))
                .AddTransition(EscapeBackward, Input(i => i.Movement.x < -inputThreshold))
                .AddTransition(Escape, Input(i => i.Movement.y < -inputThreshold));
            new[] {Escape, EscapeForward, EscapeBackward}.AddTransitionTo(Shield.Main);

            Builder.WithDefaultState(Idle);
            BuildCharacterController();
            return Builder.Build();
        }

        protected virtual void BuildCharacterController() {
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            if (_dataMap == null)
                return;
            _data = _dataMap.Select(kvp => new StateData {Name = kvp.Key, Data = kvp.Value}).ToArray();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            if (_data == null)
                return;
            _dataMap = _data.ToDictionary(s => s.Name, s => s.Data);
        }

    }

}

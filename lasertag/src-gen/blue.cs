namespace lasertag {
	using System;
	using System.Linq;
	using System.Collections.Generic;
	// Pragma and ReSharper disable all warnings for generated code
	#pragma warning disable 162
	#pragma warning disable 219
	#pragma warning disable 169
	[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "InconsistentNaming")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "UnusedParameter.Local")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "RedundantNameQualifier")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "MemberInitializerValueIgnored")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "RedundantCheckBeforeAssignment")]
	public class blue : Mars.Interfaces.Agent.IMarsDslAgent {
		private static readonly Mars.Common.Logging.ILogger _Logger = 
					Mars.Common.Logging.LoggerFactory.GetLogger(typeof(blue));
		private readonly System.Random _Random = new System.Random();
		private int __energy
			 = 100;
		public int energy { 
			get { return __energy; }
			set{
				if(__energy != value) __energy = value;
			}
		}
		private int __actionPoints
			 = 10;
		public int actionPoints { 
			get { return __actionPoints; }
			set{
				if(__actionPoints != value) __actionPoints = value;
			}
		}
		private lasertag.stance __currStance
			 = stance.Standing;
		public lasertag.stance currStance { 
			get { return __currStance; }
			set{
				if(__currStance != value) __currStance = value;
			}
		}
		private bool __inGame
			 = true;
		public bool inGame { 
			get { return __inGame; }
			set{
				if(__inGame != value) __inGame = value;
			}
		}
		private bool __wasTagged
			 = false;
		public bool wasTagged { 
			get { return __wasTagged; }
			set{
				if(__wasTagged != value) __wasTagged = value;
			}
		}
		private bool __tagged
			 = false;
		public bool tagged { 
			get { return __tagged; }
			set{
				if(__tagged != value) __tagged = value;
			}
		}
		private int __magazineCount
			 = 5;
		public int magazineCount { 
			get { return __magazineCount; }
			set{
				if(__magazineCount != value) __magazineCount = value;
			}
		}
		private double __visualRange
			 = 10;
		public double visualRange { 
			get { return __visualRange; }
			set{
				if(System.Math.Abs(__visualRange - value) > 0.0000001) __visualRange = value;
			}
		}
		private double __visibility
			 = 10;
		public double visibility { 
			get { return __visibility; }
			set{
				if(System.Math.Abs(__visibility - value) > 0.0000001) __visibility = value;
			}
		}
		private System.Tuple<lasertag.red[],lasertag.green[],lasertag.yellow[]> __enemyList
			 = null;
		internal System.Tuple<lasertag.red[],lasertag.green[],lasertag.yellow[]> enemyList { 
			get { return __enemyList; }
			set{
				if(__enemyList != value) __enemyList = value;
			}
		}
		private Mars.Components.Common.MarsList<lasertag.blue> __teamList
			 = new Mars.Components.Common.MarsList<lasertag.blue>();
		internal Mars.Components.Common.MarsList<lasertag.blue> teamList { 
			get { return __teamList; }
			set{
				if(__teamList != value) __teamList = value;
			}
		}
		private double __targetDistance
			 = default(double);
		public double targetDistance { 
			get { return __targetDistance; }
			set{
				if(System.Math.Abs(__targetDistance - value) > 0.0000001) __targetDistance = value;
			}
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public virtual void refill_action_points() 
		{
			{
			actionPoints = 10
			;}
			return;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public virtual void change_stance(lasertag.stance newStance) 
		{
			{
			if(actionPoints < 2) {
							{
							return 
							;}
					;} ;
			currStance = newStance;
			actionPoints = actionPoints - 2
			;}
			return;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public virtual void random_walk() 
		{
			{
			int _switch280_7570 = (_Random.Next(8)
			);
			bool _matched_280_7570 = false;
			bool _fallthrough_280_7570 = false;
			if(!_matched_280_7570 || _fallthrough_280_7570) {
				if(Equals(_switch280_7570, 0)) {
					_matched_280_7570 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed281_7606 = 1
					;
						
						var _entity281_7606 = this;
						
						Func<double[], bool> _predicate281_7606 = null;
						
						_Battleground._blueEnvironment.MoveTowards(_entity281_7606, Mars.Interfaces.Environment.DirectionType.Up
						, _speed281_7606);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_280_7570 = false;
				}
			}
			if(!_matched_280_7570 || _fallthrough_280_7570) {
				if(Equals(_switch280_7570, 1)) {
					_matched_280_7570 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed282_7641 = 1
					;
						
						var _entity282_7641 = this;
						
						Func<double[], bool> _predicate282_7641 = null;
						
						_Battleground._blueEnvironment.MoveTowards(_entity282_7641, Mars.Interfaces.Environment.DirectionType.UpRight
						, _speed282_7641);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_280_7570 = false;
				}
			}
			if(!_matched_280_7570 || _fallthrough_280_7570) {
				if(Equals(_switch280_7570, 2)) {
					_matched_280_7570 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed283_7687 = 1
					;
						
						var _entity283_7687 = this;
						
						Func<double[], bool> _predicate283_7687 = null;
						
						_Battleground._blueEnvironment.MoveTowards(_entity283_7687, Mars.Interfaces.Environment.DirectionType.Right
						, _speed283_7687);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_280_7570 = false;
				}
			}
			if(!_matched_280_7570 || _fallthrough_280_7570) {
				if(Equals(_switch280_7570, 3)) {
					_matched_280_7570 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed284_7724 = 1
					;
						
						var _entity284_7724 = this;
						
						Func<double[], bool> _predicate284_7724 = null;
						
						_Battleground._blueEnvironment.MoveTowards(_entity284_7724, Mars.Interfaces.Environment.DirectionType.DownRight
						, _speed284_7724);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_280_7570 = false;
				}
			}
			if(!_matched_280_7570 || _fallthrough_280_7570) {
				if(Equals(_switch280_7570, 4)) {
					_matched_280_7570 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed285_7772 = 1
					;
						
						var _entity285_7772 = this;
						
						Func<double[], bool> _predicate285_7772 = null;
						
						_Battleground._blueEnvironment.MoveTowards(_entity285_7772, Mars.Interfaces.Environment.DirectionType.Down
						, _speed285_7772);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_280_7570 = false;
				}
			}
			if(!_matched_280_7570 || _fallthrough_280_7570) {
				if(Equals(_switch280_7570, 5)) {
					_matched_280_7570 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed286_7809 = 1
					;
						
						var _entity286_7809 = this;
						
						Func<double[], bool> _predicate286_7809 = null;
						
						_Battleground._blueEnvironment.MoveTowards(_entity286_7809, Mars.Interfaces.Environment.DirectionType.DownLeft
						, _speed286_7809);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_280_7570 = false;
				}
			}
			if(!_matched_280_7570 || _fallthrough_280_7570) {
				if(Equals(_switch280_7570, 6)) {
					_matched_280_7570 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed287_7856 = 1
					;
						
						var _entity287_7856 = this;
						
						Func<double[], bool> _predicate287_7856 = null;
						
						_Battleground._blueEnvironment.MoveTowards(_entity287_7856, Mars.Interfaces.Environment.DirectionType.Left
						, _speed287_7856);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_280_7570 = false;
				}
			}
			if(!_matched_280_7570 || _fallthrough_280_7570) {
				if(Equals(_switch280_7570, 7)) {
					_matched_280_7570 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed288_7892 = 1
					;
						
						var _entity288_7892 = this;
						
						Func<double[], bool> _predicate288_7892 = null;
						
						_Battleground._blueEnvironment.MoveTowards(_entity288_7892, Mars.Interfaces.Environment.DirectionType.UpLeft
						, _speed288_7892);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_280_7570 = false;
				}
			}
			;}
			return;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public double GetVisibility() {
			{
			return visibility
			;}
			
			return default(double);;
		}
		internal bool _isAlive;
		internal int _executionFrequency;
		
		public lasertag.Battleground _Layer_ => _Battleground;
		public lasertag.Battleground _Battleground { get; set; }
		public lasertag.Battleground battleground => _Battleground;
		
		[Mars.Interfaces.LIFECapabilities.PublishForMappingInMars]
		public blue (
		System.Guid _id,
		lasertag.Battleground _layer,
		Mars.Interfaces.Layer.RegisterAgent _register,
		Mars.Interfaces.Layer.UnregisterAgent _unregister,
		Mars.Components.Environments.SpatialHashEnvironment<blue> _blueEnvironment,
		double xcor = 0, double ycor = 0, int freq = 1)
		{
			_Battleground = _layer;
			ID = _id;
			Position = Mars.Interfaces.Environment.Position.CreatePosition(xcor, ycor);
			_Random = new System.Random(ID.GetHashCode());
			_Battleground._blueEnvironment.Insert(this);
			_register(_layer, this, freq);
			_isAlive = true;
			_executionFrequency = freq;
			{
			new System.Func<System.Tuple<double,double>>(() => {
				
				var _taget257_7031 = new System.Tuple<int,int>(_Random.Next(battleground.DimensionX()
				),
				_Random.Next(battleground.DimensionY()
				)
				);
				
				var _object257_7031 = this;
				
				_Battleground._blueEnvironment.PosAt(_object257_7031, 
					_taget257_7031.Item1, _taget257_7031.Item2
				);
				return new Tuple<double, double>(Position.X, Position.Y);
			}).Invoke()
			;}
		}
		
		public void Tick()
		{
			{ if (!_isAlive) return; }
			{
			random_walk();
			change_stance(stance.Kneeling);
			refill_action_points()
			;}
		}
		
		public System.Guid ID { get; }
		public Mars.Interfaces.Environment.Position Position { get; set; }
		public bool Equals(blue other) => Equals(ID, other.ID);
		public override int GetHashCode() => ID.GetHashCode();
	}
}

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
	public class red : Mars.Interfaces.Agent.IMarsDslAgent {
		private static readonly Mars.Common.Logging.ILogger _Logger = 
					Mars.Common.Logging.LoggerFactory.GetLogger(typeof(red));
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
		private System.Tuple<lasertag.green[],lasertag.blue[],lasertag.yellow[]> __enemyList
			 = null;
		internal System.Tuple<lasertag.green[],lasertag.blue[],lasertag.yellow[]> enemyList { 
			get { return __enemyList; }
			set{
				if(__enemyList != value) __enemyList = value;
			}
		}
		private Mars.Components.Common.MarsList<lasertag.red> __teamList
			 = new Mars.Components.Common.MarsList<lasertag.red>();
		internal Mars.Components.Common.MarsList<lasertag.red> teamList { 
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
			int _switch219_5918 = (_Random.Next(8)
			);
			bool _matched_219_5918 = false;
			bool _fallthrough_219_5918 = false;
			if(!_matched_219_5918 || _fallthrough_219_5918) {
				if(Equals(_switch219_5918, 0)) {
					_matched_219_5918 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed220_5954 = 1
					;
						
						var _entity220_5954 = this;
						
						Func<double[], bool> _predicate220_5954 = null;
						
						_Battleground._redEnvironment.MoveTowards(_entity220_5954, Mars.Interfaces.Environment.DirectionType.Up
						, _speed220_5954);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_219_5918 = false;
				}
			}
			if(!_matched_219_5918 || _fallthrough_219_5918) {
				if(Equals(_switch219_5918, 1)) {
					_matched_219_5918 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed221_5989 = 1
					;
						
						var _entity221_5989 = this;
						
						Func<double[], bool> _predicate221_5989 = null;
						
						_Battleground._redEnvironment.MoveTowards(_entity221_5989, Mars.Interfaces.Environment.DirectionType.UpRight
						, _speed221_5989);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_219_5918 = false;
				}
			}
			if(!_matched_219_5918 || _fallthrough_219_5918) {
				if(Equals(_switch219_5918, 2)) {
					_matched_219_5918 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed222_6035 = 1
					;
						
						var _entity222_6035 = this;
						
						Func<double[], bool> _predicate222_6035 = null;
						
						_Battleground._redEnvironment.MoveTowards(_entity222_6035, Mars.Interfaces.Environment.DirectionType.Right
						, _speed222_6035);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_219_5918 = false;
				}
			}
			if(!_matched_219_5918 || _fallthrough_219_5918) {
				if(Equals(_switch219_5918, 3)) {
					_matched_219_5918 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed223_6072 = 1
					;
						
						var _entity223_6072 = this;
						
						Func<double[], bool> _predicate223_6072 = null;
						
						_Battleground._redEnvironment.MoveTowards(_entity223_6072, Mars.Interfaces.Environment.DirectionType.DownRight
						, _speed223_6072);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_219_5918 = false;
				}
			}
			if(!_matched_219_5918 || _fallthrough_219_5918) {
				if(Equals(_switch219_5918, 4)) {
					_matched_219_5918 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed224_6120 = 1
					;
						
						var _entity224_6120 = this;
						
						Func<double[], bool> _predicate224_6120 = null;
						
						_Battleground._redEnvironment.MoveTowards(_entity224_6120, Mars.Interfaces.Environment.DirectionType.Down
						, _speed224_6120);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_219_5918 = false;
				}
			}
			if(!_matched_219_5918 || _fallthrough_219_5918) {
				if(Equals(_switch219_5918, 5)) {
					_matched_219_5918 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed225_6157 = 1
					;
						
						var _entity225_6157 = this;
						
						Func<double[], bool> _predicate225_6157 = null;
						
						_Battleground._redEnvironment.MoveTowards(_entity225_6157, Mars.Interfaces.Environment.DirectionType.DownLeft
						, _speed225_6157);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_219_5918 = false;
				}
			}
			if(!_matched_219_5918 || _fallthrough_219_5918) {
				if(Equals(_switch219_5918, 6)) {
					_matched_219_5918 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed226_6204 = 1
					;
						
						var _entity226_6204 = this;
						
						Func<double[], bool> _predicate226_6204 = null;
						
						_Battleground._redEnvironment.MoveTowards(_entity226_6204, Mars.Interfaces.Environment.DirectionType.Left
						, _speed226_6204);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_219_5918 = false;
				}
			}
			if(!_matched_219_5918 || _fallthrough_219_5918) {
				if(Equals(_switch219_5918, 7)) {
					_matched_219_5918 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed227_6240 = 1
					;
						
						var _entity227_6240 = this;
						
						Func<double[], bool> _predicate227_6240 = null;
						
						_Battleground._redEnvironment.MoveTowards(_entity227_6240, Mars.Interfaces.Environment.DirectionType.UpLeft
						, _speed227_6240);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_219_5918 = false;
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
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public double GetX() {
			{
			return this.Position.X
			;}
			
			return default(double);;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public double GetY() {
			{
			return this.Position.Y
			;}
			
			return default(double);;
		}
		internal bool _isAlive;
		internal int _executionFrequency;
		
		public lasertag.Battleground _Layer_ => _Battleground;
		public lasertag.Battleground _Battleground { get; set; }
		public lasertag.Battleground battleground => _Battleground;
		
		[Mars.Interfaces.LIFECapabilities.PublishForMappingInMars]
		public red (
		System.Guid _id,
		lasertag.Battleground _layer,
		Mars.Interfaces.Layer.RegisterAgent _register,
		Mars.Interfaces.Layer.UnregisterAgent _unregister,
		Mars.Components.Environments.SpatialHashEnvironment<red> _redEnvironment,
		double xcor = 0, double ycor = 0, int freq = 1)
		{
			_Battleground = _layer;
			ID = _id;
			Position = Mars.Interfaces.Environment.Position.CreatePosition(xcor, ycor);
			_Random = new System.Random(ID.GetHashCode());
			_Battleground._redEnvironment.Insert(this);
			_register(_layer, this, freq);
			_isAlive = true;
			_executionFrequency = freq;
			{
			new System.Func<System.Tuple<double,double>>(() => {
				
				var _taget196_5437 = new System.Tuple<int,int>(3,1);
				
				var _object196_5437 = this;
				
				_Battleground._redEnvironment.PosAt(_object196_5437, 
					_taget196_5437.Item1, _taget196_5437.Item2
				);
				return new Tuple<double, double>(Position.X, Position.Y);
			}).Invoke()
			;}
		}
		
		public void Tick()
		{
			{ if (!_isAlive) return; }
			{
			change_stance(stance.Kneeling);
			refill_action_points()
			;}
		}
		
		public System.Guid ID { get; }
		public Mars.Interfaces.Environment.Position Position { get; set; }
		public bool Equals(red other) => Equals(ID, other.ID);
		public override int GetHashCode() => ID.GetHashCode();
	}
}

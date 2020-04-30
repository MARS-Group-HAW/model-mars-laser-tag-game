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
	public class yellow : Mars.Interfaces.Agent.IMarsDslAgent {
		private static readonly Mars.Common.Logging.ILogger _Logger = 
					Mars.Common.Logging.LoggerFactory.GetLogger(typeof(yellow));
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
		private System.Tuple<lasertag.red[],lasertag.blue[],lasertag.green[]> __enemyList
			 = null;
		internal System.Tuple<lasertag.red[],lasertag.blue[],lasertag.green[]> enemyList { 
			get { return __enemyList; }
			set{
				if(__enemyList != value) __enemyList = value;
			}
		}
		private Mars.Components.Common.MarsList<lasertag.yellow> __teamList
			 = new Mars.Components.Common.MarsList<lasertag.yellow>();
		internal Mars.Components.Common.MarsList<lasertag.yellow> teamList { 
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
			int _switch335_9134 = (_Random.Next(8)
			);
			bool _matched_335_9134 = false;
			bool _fallthrough_335_9134 = false;
			if(!_matched_335_9134 || _fallthrough_335_9134) {
				if(Equals(_switch335_9134, 0)) {
					_matched_335_9134 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed336_9170 = 1
					;
						
						var _entity336_9170 = this;
						
						Func<double[], bool> _predicate336_9170 = null;
						
						_Battleground._yellowEnvironment.MoveTowards(_entity336_9170, Mars.Interfaces.Environment.DirectionType.Up
						, _speed336_9170);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_335_9134 = false;
				}
			}
			if(!_matched_335_9134 || _fallthrough_335_9134) {
				if(Equals(_switch335_9134, 1)) {
					_matched_335_9134 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed337_9205 = 1
					;
						
						var _entity337_9205 = this;
						
						Func<double[], bool> _predicate337_9205 = null;
						
						_Battleground._yellowEnvironment.MoveTowards(_entity337_9205, Mars.Interfaces.Environment.DirectionType.UpRight
						, _speed337_9205);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_335_9134 = false;
				}
			}
			if(!_matched_335_9134 || _fallthrough_335_9134) {
				if(Equals(_switch335_9134, 2)) {
					_matched_335_9134 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed338_9251 = 1
					;
						
						var _entity338_9251 = this;
						
						Func<double[], bool> _predicate338_9251 = null;
						
						_Battleground._yellowEnvironment.MoveTowards(_entity338_9251, Mars.Interfaces.Environment.DirectionType.Right
						, _speed338_9251);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_335_9134 = false;
				}
			}
			if(!_matched_335_9134 || _fallthrough_335_9134) {
				if(Equals(_switch335_9134, 3)) {
					_matched_335_9134 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed339_9288 = 1
					;
						
						var _entity339_9288 = this;
						
						Func<double[], bool> _predicate339_9288 = null;
						
						_Battleground._yellowEnvironment.MoveTowards(_entity339_9288, Mars.Interfaces.Environment.DirectionType.DownRight
						, _speed339_9288);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_335_9134 = false;
				}
			}
			if(!_matched_335_9134 || _fallthrough_335_9134) {
				if(Equals(_switch335_9134, 4)) {
					_matched_335_9134 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed340_9336 = 1
					;
						
						var _entity340_9336 = this;
						
						Func<double[], bool> _predicate340_9336 = null;
						
						_Battleground._yellowEnvironment.MoveTowards(_entity340_9336, Mars.Interfaces.Environment.DirectionType.Down
						, _speed340_9336);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_335_9134 = false;
				}
			}
			if(!_matched_335_9134 || _fallthrough_335_9134) {
				if(Equals(_switch335_9134, 5)) {
					_matched_335_9134 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed341_9373 = 1
					;
						
						var _entity341_9373 = this;
						
						Func<double[], bool> _predicate341_9373 = null;
						
						_Battleground._yellowEnvironment.MoveTowards(_entity341_9373, Mars.Interfaces.Environment.DirectionType.DownLeft
						, _speed341_9373);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_335_9134 = false;
				}
			}
			if(!_matched_335_9134 || _fallthrough_335_9134) {
				if(Equals(_switch335_9134, 6)) {
					_matched_335_9134 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed342_9420 = 1
					;
						
						var _entity342_9420 = this;
						
						Func<double[], bool> _predicate342_9420 = null;
						
						_Battleground._yellowEnvironment.MoveTowards(_entity342_9420, Mars.Interfaces.Environment.DirectionType.Left
						, _speed342_9420);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_335_9134 = false;
				}
			}
			if(!_matched_335_9134 || _fallthrough_335_9134) {
				if(Equals(_switch335_9134, 7)) {
					_matched_335_9134 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed343_9456 = 1
					;
						
						var _entity343_9456 = this;
						
						Func<double[], bool> _predicate343_9456 = null;
						
						_Battleground._yellowEnvironment.MoveTowards(_entity343_9456, Mars.Interfaces.Environment.DirectionType.UpLeft
						, _speed343_9456);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_335_9134 = false;
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
		public yellow (
		System.Guid _id,
		lasertag.Battleground _layer,
		Mars.Interfaces.Layer.RegisterAgent _register,
		Mars.Interfaces.Layer.UnregisterAgent _unregister,
		Mars.Components.Environments.SpatialHashEnvironment<yellow> _yellowEnvironment,
		double xcor = 0, double ycor = 0, int freq = 1)
		{
			_Battleground = _layer;
			ID = _id;
			Position = Mars.Interfaces.Environment.Position.CreatePosition(xcor, ycor);
			_Random = new System.Random(ID.GetHashCode());
			_Battleground._yellowEnvironment.Insert(this);
			_register(_layer, this, freq);
			_isAlive = true;
			_executionFrequency = freq;
			{
			new System.Func<System.Tuple<double,double>>(() => {
				
				var _taget312_8595 = new System.Tuple<int,int>(_Random.Next(battleground.DimensionX()
				),
				_Random.Next(battleground.DimensionY()
				)
				);
				
				var _object312_8595 = this;
				
				_Battleground._yellowEnvironment.PosAt(_object312_8595, 
					_taget312_8595.Item1, _taget312_8595.Item2
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
		public bool Equals(yellow other) => Equals(ID, other.ID);
		public override int GetHashCode() => ID.GetHashCode();
	}
}

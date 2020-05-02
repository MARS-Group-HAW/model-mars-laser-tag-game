namespace lasertag {
	using System;
	using System.Linq;
	using System.Collections.Generic;
	// Pragma and ReSharper disable all warnings for generated code
	#pragma warning disable 162
	#pragma warning disable 219
	#pragma warning disable 169
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
		public void refill_points() 
		{
			{
			actionPoints = 10
			;}
			return;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void change_stance(lasertag.stance newStance) 
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
		public void random_walk() 
		{
			{
			int _switch363_11437 = (_Random.Next(8)
			);
			bool _matched_363_11437 = false;
			bool _fallthrough_363_11437 = false;
			if(!_matched_363_11437 || _fallthrough_363_11437) {
				if(Equals(_switch363_11437, 0)) {
					_matched_363_11437 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed364_11473 = 1
					;
						
						var _entity364_11473 = this;
						
						Func<double[], bool> _predicate364_11473 = null;
						
						_Battleground._redEnvironment.MoveTowards(_entity364_11473, Mars.Interfaces.Environment.DirectionType.Up
						, _speed364_11473);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_363_11437 = false;
				}
			}
			if(!_matched_363_11437 || _fallthrough_363_11437) {
				if(Equals(_switch363_11437, 1)) {
					_matched_363_11437 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed365_11509 = 1
					;
						
						var _entity365_11509 = this;
						
						Func<double[], bool> _predicate365_11509 = null;
						
						_Battleground._redEnvironment.MoveTowards(_entity365_11509, Mars.Interfaces.Environment.DirectionType.UpRight
						, _speed365_11509);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_363_11437 = false;
				}
			}
			if(!_matched_363_11437 || _fallthrough_363_11437) {
				if(Equals(_switch363_11437, 2)) {
					_matched_363_11437 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed366_11556 = 1
					;
						
						var _entity366_11556 = this;
						
						Func<double[], bool> _predicate366_11556 = null;
						
						_Battleground._redEnvironment.MoveTowards(_entity366_11556, Mars.Interfaces.Environment.DirectionType.Right
						, _speed366_11556);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_363_11437 = false;
				}
			}
			if(!_matched_363_11437 || _fallthrough_363_11437) {
				if(Equals(_switch363_11437, 3)) {
					_matched_363_11437 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed367_11594 = 1
					;
						
						var _entity367_11594 = this;
						
						Func<double[], bool> _predicate367_11594 = null;
						
						_Battleground._redEnvironment.MoveTowards(_entity367_11594, Mars.Interfaces.Environment.DirectionType.DownRight
						, _speed367_11594);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_363_11437 = false;
				}
			}
			if(!_matched_363_11437 || _fallthrough_363_11437) {
				if(Equals(_switch363_11437, 4)) {
					_matched_363_11437 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed368_11643 = 1
					;
						
						var _entity368_11643 = this;
						
						Func<double[], bool> _predicate368_11643 = null;
						
						_Battleground._redEnvironment.MoveTowards(_entity368_11643, Mars.Interfaces.Environment.DirectionType.Down
						, _speed368_11643);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_363_11437 = false;
				}
			}
			if(!_matched_363_11437 || _fallthrough_363_11437) {
				if(Equals(_switch363_11437, 5)) {
					_matched_363_11437 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed369_11681 = 1
					;
						
						var _entity369_11681 = this;
						
						Func<double[], bool> _predicate369_11681 = null;
						
						_Battleground._redEnvironment.MoveTowards(_entity369_11681, Mars.Interfaces.Environment.DirectionType.DownLeft
						, _speed369_11681);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_363_11437 = false;
				}
			}
			if(!_matched_363_11437 || _fallthrough_363_11437) {
				if(Equals(_switch363_11437, 6)) {
					_matched_363_11437 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed370_11729 = 1
					;
						
						var _entity370_11729 = this;
						
						Func<double[], bool> _predicate370_11729 = null;
						
						_Battleground._redEnvironment.MoveTowards(_entity370_11729, Mars.Interfaces.Environment.DirectionType.Left
						, _speed370_11729);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_363_11437 = false;
				}
			}
			if(!_matched_363_11437 || _fallthrough_363_11437) {
				if(Equals(_switch363_11437, 7)) {
					_matched_363_11437 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed371_11766 = 1
					;
						
						var _entity371_11766 = this;
						
						Func<double[], bool> _predicate371_11766 = null;
						
						_Battleground._redEnvironment.MoveTowards(_entity371_11766, Mars.Interfaces.Environment.DirectionType.UpLeft
						, _speed371_11766);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_363_11437 = false;
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
				
				var _taget344_11168 = new System.Tuple<int,int>(3,1);
				
				var _object344_11168 = this;
				
				_Battleground._redEnvironment.PosAt(_object344_11168, 
					_taget344_11168.Item1, _taget344_11168.Item2
				);
				return new Tuple<double, double>(Position.X, Position.Y);
			}).Invoke()
			;}
		}
		
		public void Tick()
		{
			{ if (!_isAlive) return; }
			{
			}
		}
		
		public System.Guid ID { get; }
		public Mars.Interfaces.Environment.Position Position { get; set; }
		public bool Equals(red other) => Equals(ID, other.ID);
		public override int GetHashCode() => ID.GetHashCode();
	}
}

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
			int _switch477_14031 = (_Random.Next(8)
			);
			bool _matched_477_14031 = false;
			bool _fallthrough_477_14031 = false;
			if(!_matched_477_14031 || _fallthrough_477_14031) {
				if(Equals(_switch477_14031, 0)) {
					_matched_477_14031 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed478_14067 = 1
					;
						
						var _entity478_14067 = this;
						
						Func<double[], bool> _predicate478_14067 = null;
						
						_Battleground._yellowEnvironment.MoveTowards(_entity478_14067, Mars.Interfaces.Environment.DirectionType.Up
						, _speed478_14067);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_477_14031 = false;
				}
			}
			if(!_matched_477_14031 || _fallthrough_477_14031) {
				if(Equals(_switch477_14031, 1)) {
					_matched_477_14031 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed479_14103 = 1
					;
						
						var _entity479_14103 = this;
						
						Func<double[], bool> _predicate479_14103 = null;
						
						_Battleground._yellowEnvironment.MoveTowards(_entity479_14103, Mars.Interfaces.Environment.DirectionType.UpRight
						, _speed479_14103);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_477_14031 = false;
				}
			}
			if(!_matched_477_14031 || _fallthrough_477_14031) {
				if(Equals(_switch477_14031, 2)) {
					_matched_477_14031 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed480_14150 = 1
					;
						
						var _entity480_14150 = this;
						
						Func<double[], bool> _predicate480_14150 = null;
						
						_Battleground._yellowEnvironment.MoveTowards(_entity480_14150, Mars.Interfaces.Environment.DirectionType.Right
						, _speed480_14150);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_477_14031 = false;
				}
			}
			if(!_matched_477_14031 || _fallthrough_477_14031) {
				if(Equals(_switch477_14031, 3)) {
					_matched_477_14031 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed481_14188 = 1
					;
						
						var _entity481_14188 = this;
						
						Func<double[], bool> _predicate481_14188 = null;
						
						_Battleground._yellowEnvironment.MoveTowards(_entity481_14188, Mars.Interfaces.Environment.DirectionType.DownRight
						, _speed481_14188);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_477_14031 = false;
				}
			}
			if(!_matched_477_14031 || _fallthrough_477_14031) {
				if(Equals(_switch477_14031, 4)) {
					_matched_477_14031 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed482_14237 = 1
					;
						
						var _entity482_14237 = this;
						
						Func<double[], bool> _predicate482_14237 = null;
						
						_Battleground._yellowEnvironment.MoveTowards(_entity482_14237, Mars.Interfaces.Environment.DirectionType.Down
						, _speed482_14237);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_477_14031 = false;
				}
			}
			if(!_matched_477_14031 || _fallthrough_477_14031) {
				if(Equals(_switch477_14031, 5)) {
					_matched_477_14031 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed483_14275 = 1
					;
						
						var _entity483_14275 = this;
						
						Func<double[], bool> _predicate483_14275 = null;
						
						_Battleground._yellowEnvironment.MoveTowards(_entity483_14275, Mars.Interfaces.Environment.DirectionType.DownLeft
						, _speed483_14275);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_477_14031 = false;
				}
			}
			if(!_matched_477_14031 || _fallthrough_477_14031) {
				if(Equals(_switch477_14031, 6)) {
					_matched_477_14031 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed484_14323 = 1
					;
						
						var _entity484_14323 = this;
						
						Func<double[], bool> _predicate484_14323 = null;
						
						_Battleground._yellowEnvironment.MoveTowards(_entity484_14323, Mars.Interfaces.Environment.DirectionType.Left
						, _speed484_14323);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_477_14031 = false;
				}
			}
			if(!_matched_477_14031 || _fallthrough_477_14031) {
				if(Equals(_switch477_14031, 7)) {
					_matched_477_14031 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed485_14360 = 1
					;
						
						var _entity485_14360 = this;
						
						Func<double[], bool> _predicate485_14360 = null;
						
						_Battleground._yellowEnvironment.MoveTowards(_entity485_14360, Mars.Interfaces.Environment.DirectionType.UpLeft
						, _speed485_14360);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_477_14031 = false;
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
				
				var _taget457_13759 = new System.Tuple<int,int>(7,1);
				
				var _object457_13759 = this;
				
				_Battleground._yellowEnvironment.PosAt(_object457_13759, 
					_taget457_13759.Item1, _taget457_13759.Item2
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
		public bool Equals(yellow other) => Equals(ID, other.ID);
		public override int GetHashCode() => ID.GetHashCode();
	}
}

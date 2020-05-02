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
			int _switch424_12782 = (_Random.Next(8)
			);
			bool _matched_424_12782 = false;
			bool _fallthrough_424_12782 = false;
			if(!_matched_424_12782 || _fallthrough_424_12782) {
				if(Equals(_switch424_12782, 0)) {
					_matched_424_12782 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed425_12818 = 1
					;
						
						var _entity425_12818 = this;
						
						Func<double[], bool> _predicate425_12818 = null;
						
						_Battleground._blueEnvironment.MoveTowards(_entity425_12818, Mars.Interfaces.Environment.DirectionType.Up
						, _speed425_12818);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_424_12782 = false;
				}
			}
			if(!_matched_424_12782 || _fallthrough_424_12782) {
				if(Equals(_switch424_12782, 1)) {
					_matched_424_12782 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed426_12854 = 1
					;
						
						var _entity426_12854 = this;
						
						Func<double[], bool> _predicate426_12854 = null;
						
						_Battleground._blueEnvironment.MoveTowards(_entity426_12854, Mars.Interfaces.Environment.DirectionType.UpRight
						, _speed426_12854);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_424_12782 = false;
				}
			}
			if(!_matched_424_12782 || _fallthrough_424_12782) {
				if(Equals(_switch424_12782, 2)) {
					_matched_424_12782 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed427_12901 = 1
					;
						
						var _entity427_12901 = this;
						
						Func<double[], bool> _predicate427_12901 = null;
						
						_Battleground._blueEnvironment.MoveTowards(_entity427_12901, Mars.Interfaces.Environment.DirectionType.Right
						, _speed427_12901);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_424_12782 = false;
				}
			}
			if(!_matched_424_12782 || _fallthrough_424_12782) {
				if(Equals(_switch424_12782, 3)) {
					_matched_424_12782 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed428_12939 = 1
					;
						
						var _entity428_12939 = this;
						
						Func<double[], bool> _predicate428_12939 = null;
						
						_Battleground._blueEnvironment.MoveTowards(_entity428_12939, Mars.Interfaces.Environment.DirectionType.DownRight
						, _speed428_12939);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_424_12782 = false;
				}
			}
			if(!_matched_424_12782 || _fallthrough_424_12782) {
				if(Equals(_switch424_12782, 4)) {
					_matched_424_12782 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed429_12988 = 1
					;
						
						var _entity429_12988 = this;
						
						Func<double[], bool> _predicate429_12988 = null;
						
						_Battleground._blueEnvironment.MoveTowards(_entity429_12988, Mars.Interfaces.Environment.DirectionType.Down
						, _speed429_12988);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_424_12782 = false;
				}
			}
			if(!_matched_424_12782 || _fallthrough_424_12782) {
				if(Equals(_switch424_12782, 5)) {
					_matched_424_12782 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed430_13026 = 1
					;
						
						var _entity430_13026 = this;
						
						Func<double[], bool> _predicate430_13026 = null;
						
						_Battleground._blueEnvironment.MoveTowards(_entity430_13026, Mars.Interfaces.Environment.DirectionType.DownLeft
						, _speed430_13026);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_424_12782 = false;
				}
			}
			if(!_matched_424_12782 || _fallthrough_424_12782) {
				if(Equals(_switch424_12782, 6)) {
					_matched_424_12782 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed431_13074 = 1
					;
						
						var _entity431_13074 = this;
						
						Func<double[], bool> _predicate431_13074 = null;
						
						_Battleground._blueEnvironment.MoveTowards(_entity431_13074, Mars.Interfaces.Environment.DirectionType.Left
						, _speed431_13074);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_424_12782 = false;
				}
			}
			if(!_matched_424_12782 || _fallthrough_424_12782) {
				if(Equals(_switch424_12782, 7)) {
					_matched_424_12782 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed432_13111 = 1
					;
						
						var _entity432_13111 = this;
						
						Func<double[], bool> _predicate432_13111 = null;
						
						_Battleground._blueEnvironment.MoveTowards(_entity432_13111, Mars.Interfaces.Environment.DirectionType.UpLeft
						, _speed432_13111);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_424_12782 = false;
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
				
				var _taget404_12510 = new System.Tuple<int,int>(7,2);
				
				var _object404_12510 = this;
				
				_Battleground._blueEnvironment.PosAt(_object404_12510, 
					_taget404_12510.Item1, _taget404_12510.Item2
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
		public bool Equals(blue other) => Equals(ID, other.ID);
		public override int GetHashCode() => ID.GetHashCode();
	}
}

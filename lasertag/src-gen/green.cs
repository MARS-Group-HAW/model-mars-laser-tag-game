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
	public class green : Mars.Interfaces.Agent.IMarsDslAgent {
		private static readonly Mars.Common.Logging.ILogger _Logger = 
					Mars.Common.Logging.LoggerFactory.GetLogger(typeof(green));
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
			 = 2;
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
		private System.Tuple<Mars.Components.Common.MarsList<lasertag.red>,lasertag.blue[],lasertag.yellow[]> __enemyList
			 = default(System.Tuple<Mars.Components.Common.MarsList<lasertag.red>,lasertag.blue[],lasertag.yellow[]>);
		internal System.Tuple<Mars.Components.Common.MarsList<lasertag.red>,lasertag.blue[],lasertag.yellow[]> enemyList { 
			get { return __enemyList; }
			set{
				if(__enemyList != value) __enemyList = value;
			}
		}
		private lasertag.green[] __teamList
			 = default(lasertag.green[]);
		internal lasertag.green[] teamList { 
			get { return __teamList; }
			set{
				if(__teamList != value) __teamList = value;
			}
		}
		private Mars.Components.Common.MarsList<System.Tuple<double,double>> __wallList
			 = default(Mars.Components.Common.MarsList<System.Tuple<double,double>>);
		internal Mars.Components.Common.MarsList<System.Tuple<double,double>> wallList { 
			get { return __wallList; }
			set{
				if(__wallList != value) __wallList = value;
			}
		}
		private Mars.Components.Common.MarsList<lasertag.red> __redList
			 = new Mars.Components.Common.MarsList<lasertag.red>();
		internal Mars.Components.Common.MarsList<lasertag.red> redList { 
			get { return __redList; }
			set{
				if(__redList != value) __redList = value;
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
			actionPoints = actionPoints - 2;
			lasertag.stance _switch58_1524 = (currStance);
			bool _matched_58_1524 = false;
			bool _fallthrough_58_1524 = false;
			if(!_matched_58_1524 || _fallthrough_58_1524) {
				if(Equals(_switch58_1524, stance.Standing)) {
					_matched_58_1524 = true;
					{
					visualRange = 10;
					visibility = 10
					;}
				} else {
					_fallthrough_58_1524 = false;
				}
			}
			if(!_matched_58_1524 || _fallthrough_58_1524) {
				if(Equals(_switch58_1524, stance.Kneeling)) {
					_matched_58_1524 = true;
					{
					visualRange = 8;
					visibility = 8
					;}
				} else {
					_fallthrough_58_1524 = false;
				}
			}
			if(!_matched_58_1524 || _fallthrough_58_1524) {
				if(Equals(_switch58_1524, stance.Lying)) {
					_matched_58_1524 = true;
					{
					visualRange = 5;
					visibility = 5
					;}
				} else {
					_fallthrough_58_1524 = false;
				}
			}
			;}
			return;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public virtual void explore_env() 
		{
			{
			explore_team();
			explore_enemies();
			explore_walls()
			;}
			return;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public virtual void explore_walls() 
		{
			{
			wallList = new Mars.Components.Common.MarsList<System.Tuple<double,double>>();
			for(double x = this.Position.X - visualRange;
					 x <= this.Position.X + visualRange;
					 x++){
					 	{
					 	for(double y = this.Position.Y + visualRange;
					 			 y >= this.Position.Y - visualRange;
					 			 y--){
					 			 	{
					 			 	if(Equals(battleground.GetIntegerValue(x,y)
					 			 	, 1)) {
					 			 					{
					 			 					wallList.Add(new System.Tuple<double,double>(x,y));
					 			 					System.Console.WriteLine(new System.Tuple<double,double>(x,y));
					 			 					;}
					 			 			;} 
					 			 	;}
					 			 }
					 	;}
					 }
			;}
			return;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public virtual void explore_team() 
		{
			{
			teamList = new System.Func<lasertag.green[]>(() => {
				
				var _sourceMapped94_2340 = this.Position;
				var _source94_2340 = _sourceMapped94_2340;
				var _range94_2340 = -1;
							
				Func<lasertag.green, bool> _predicate94_2340 = null;
				Func<lasertag.green, bool> _predicateMod94_2340 = new Func<lasertag.green, bool>(_it => 
				{
					if (_it?.ID == this.ID)
					{
						return false;
					} else if (_predicate94_2340 != null)
					{
						return _predicate94_2340.Invoke(_it);
					} else return true;
				});
				
				return _Battleground._greenEnvironment.Explore(_source94_2340 , _range94_2340, -1, _predicate94_2340).ToArray();
			}).Invoke()
			;}
			return;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public virtual void explore_enemies() 
		{
			{
			lasertag.red[] tmpRed = new System.Func<lasertag.red[]>(() => {
				
				var _sourceMapped99_2518 = this.Position;
				var _source99_2518 = _sourceMapped99_2518;
				var _range99_2518 = -1;
							
				Func<lasertag.red, bool> _predicate99_2518 = new Func<lasertag.red,bool>((lasertag.red x) => 
				 {
						{
						return (new Func<double>(() => {
						var _target99_2550 = x;
						if (_target99_2550 == null) return 0.0;
						return Mars.Mathematics.Distance.Calculate(Mars.Mathematics.SpaceDistanceMetric.Chebyshev, this.Position.PositionArray, _target99_2550.Position.PositionArray);
						}).Invoke()) <= visualRange && (new Func<double>(() => {
						var _target99_2582 = x;
						if (_target99_2582 == null) return 0.0;
						return Mars.Mathematics.Distance.Calculate(Mars.Mathematics.SpaceDistanceMetric.Chebyshev, this.Position.PositionArray, _target99_2582.Position.PositionArray);
						}).Invoke()) <= x.GetVisibility()
						;}
						;
						return default(bool);;
				});
				Func<lasertag.red, bool> _predicateMod99_2518 = new Func<lasertag.red, bool>(_it => 
				{
					if (_it?.ID == this.ID)
					{
						return false;
					} else if (_predicate99_2518 != null)
					{
						return _predicate99_2518.Invoke(_it);
					} else return true;
				});
				
				return _Battleground._redEnvironment.Explore(_source99_2518 , _range99_2518, -1, _predicate99_2518).ToArray();
			}).Invoke();
			lasertag.blue[] tmpBlue = new System.Func<lasertag.blue[]>(() => {
				
				var _sourceMapped100_2631 = this.Position;
				var _source100_2631 = _sourceMapped100_2631;
				var _range100_2631 = -1;
							
				Func<lasertag.blue, bool> _predicate100_2631 = new Func<lasertag.blue,bool>((lasertag.blue x) => 
				 {
						{
						return (new Func<double>(() => {
						var _target100_2664 = x;
						if (_target100_2664 == null) return 0.0;
						return Mars.Mathematics.Distance.Calculate(Mars.Mathematics.SpaceDistanceMetric.Chebyshev, this.Position.PositionArray, _target100_2664.Position.PositionArray);
						}).Invoke()) <= visualRange && (new Func<double>(() => {
						var _target100_2696 = x;
						if (_target100_2696 == null) return 0.0;
						return Mars.Mathematics.Distance.Calculate(Mars.Mathematics.SpaceDistanceMetric.Chebyshev, this.Position.PositionArray, _target100_2696.Position.PositionArray);
						}).Invoke()) <= x.GetVisibility()
						;}
						;
						return default(bool);;
				});
				Func<lasertag.blue, bool> _predicateMod100_2631 = new Func<lasertag.blue, bool>(_it => 
				{
					if (_it?.ID == this.ID)
					{
						return false;
					} else if (_predicate100_2631 != null)
					{
						return _predicate100_2631.Invoke(_it);
					} else return true;
				});
				
				return _Battleground._blueEnvironment.Explore(_source100_2631 , _range100_2631, -1, _predicate100_2631).ToArray();
			}).Invoke();
			lasertag.yellow[] tmpYellow = new System.Func<lasertag.yellow[]>(() => {
				
				var _sourceMapped101_2747 = this.Position;
				var _source101_2747 = _sourceMapped101_2747;
				var _range101_2747 = -1;
							
				Func<lasertag.yellow, bool> _predicate101_2747 = new Func<lasertag.yellow,bool>((lasertag.yellow x) => 
				 {
						{
						return (new Func<double>(() => {
						var _target101_2782 = x;
						if (_target101_2782 == null) return 0.0;
						return Mars.Mathematics.Distance.Calculate(Mars.Mathematics.SpaceDistanceMetric.Chebyshev, this.Position.PositionArray, _target101_2782.Position.PositionArray);
						}).Invoke()) <= visualRange && (new Func<double>(() => {
						var _target101_2814 = x;
						if (_target101_2814 == null) return 0.0;
						return Mars.Mathematics.Distance.Calculate(Mars.Mathematics.SpaceDistanceMetric.Chebyshev, this.Position.PositionArray, _target101_2814.Position.PositionArray);
						}).Invoke()) <= x.GetVisibility()
						;}
						;
						return default(bool);;
				});
				Func<lasertag.yellow, bool> _predicateMod101_2747 = new Func<lasertag.yellow, bool>(_it => 
				{
					if (_it?.ID == this.ID)
					{
						return false;
					} else if (_predicate101_2747 != null)
					{
						return _predicate101_2747.Invoke(_it);
					} else return true;
				});
				
				return _Battleground._yellowEnvironment.Explore(_source101_2747 , _range101_2747, -1, _predicate101_2747).ToArray();
			}).Invoke();
			foreach ( var red in tmpRed ) {
						{
						if(Equals(this.Position.X, red.GetX()
						)) {
										{
										if(this.Position.Y > red.GetY()
										) {
														{
														foreach ( var wall in wallList ) {
																	{
																	if(!(Equals(wall.Item1
																	, this.Position.X) && wall.Item2
																	 > red.GetY()
																	 && wall.Item2
																	 < this.Position.Y)) {
																					{
																					redList.Add(red)
																					;}
																			;} 
																	;}
																}
														;}
												;} ;
										if(this.Position.Y < red.GetY()
										) {
														{
														foreach ( var wall in wallList ) {
																	{
																	if(!(Equals(wall.Item1
																	, this.Position.X) && wall.Item2
																	 < red.GetY()
																	 && wall.Item2
																	 > this.Position.Y)) {
																					{
																					redList.Add(red)
																					;}
																			;} 
																	;}
																}
														;}
												;} 
										;}
								;} else {
										if(Equals(this.Position.Y, red.GetY()
										)) {
														{
														if(this.Position.X > red.GetX()
														) {
																		{
																		foreach ( var wall in wallList ) {
																					{
																					if(!(Equals(wall.Item2
																					, this.Position.Y) && wall.Item1
																					 > red.GetX()
																					 && wall.Item1
																					 < this.Position.X)) {
																									{
																									redList.Add(red)
																									;}
																							;} 
																					;}
																				}
																		;}
																;} ;
														if(this.Position.X < red.GetX()
														) {
																		{
																		foreach ( var wall in wallList ) {
																					{
																					if(!(Equals(wall.Item2
																					, this.Position.Y) && wall.Item1
																					 < red.GetX()
																					 && wall.Item1
																					 > this.Position.X)) {
																									{
																									redList.Add(red)
																									;}
																							;} 
																					;}
																				}
																		;}
																;} 
														;}
												;} else {
														{
														}
													;}
									;}
						;}
					};
			enemyList = new System.Tuple<Mars.Components.Common.MarsList<lasertag.red>,lasertag.blue[],lasertag.yellow[]>(redList,tmpBlue,tmpYellow)
			;}
			return;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public virtual void move_me() 
		{
			{
			int _switch154_4299 = (_Random.Next(8)
			);
			bool _matched_154_4299 = false;
			bool _fallthrough_154_4299 = false;
			if(!_matched_154_4299 || _fallthrough_154_4299) {
				if(Equals(_switch154_4299, 0)) {
					_matched_154_4299 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed155_4335 = 1
					;
						
						var _entity155_4335 = this;
						
						Func<double[], bool> _predicate155_4335 = null;
						
						_Battleground._greenEnvironment.MoveTowards(_entity155_4335, Mars.Interfaces.Environment.DirectionType.Up
						, _speed155_4335);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_154_4299 = false;
				}
			}
			if(!_matched_154_4299 || _fallthrough_154_4299) {
				if(Equals(_switch154_4299, 1)) {
					_matched_154_4299 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed156_4370 = 1
					;
						
						var _entity156_4370 = this;
						
						Func<double[], bool> _predicate156_4370 = null;
						
						_Battleground._greenEnvironment.MoveTowards(_entity156_4370, Mars.Interfaces.Environment.DirectionType.UpRight
						, _speed156_4370);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_154_4299 = false;
				}
			}
			if(!_matched_154_4299 || _fallthrough_154_4299) {
				if(Equals(_switch154_4299, 2)) {
					_matched_154_4299 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed157_4416 = 1
					;
						
						var _entity157_4416 = this;
						
						Func<double[], bool> _predicate157_4416 = null;
						
						_Battleground._greenEnvironment.MoveTowards(_entity157_4416, Mars.Interfaces.Environment.DirectionType.Right
						, _speed157_4416);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_154_4299 = false;
				}
			}
			if(!_matched_154_4299 || _fallthrough_154_4299) {
				if(Equals(_switch154_4299, 3)) {
					_matched_154_4299 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed158_4453 = 1
					;
						
						var _entity158_4453 = this;
						
						Func<double[], bool> _predicate158_4453 = null;
						
						_Battleground._greenEnvironment.MoveTowards(_entity158_4453, Mars.Interfaces.Environment.DirectionType.DownRight
						, _speed158_4453);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_154_4299 = false;
				}
			}
			if(!_matched_154_4299 || _fallthrough_154_4299) {
				if(Equals(_switch154_4299, 4)) {
					_matched_154_4299 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed159_4501 = 1
					;
						
						var _entity159_4501 = this;
						
						Func<double[], bool> _predicate159_4501 = null;
						
						_Battleground._greenEnvironment.MoveTowards(_entity159_4501, Mars.Interfaces.Environment.DirectionType.Down
						, _speed159_4501);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_154_4299 = false;
				}
			}
			if(!_matched_154_4299 || _fallthrough_154_4299) {
				if(Equals(_switch154_4299, 5)) {
					_matched_154_4299 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed160_4538 = 1
					;
						
						var _entity160_4538 = this;
						
						Func<double[], bool> _predicate160_4538 = null;
						
						_Battleground._greenEnvironment.MoveTowards(_entity160_4538, Mars.Interfaces.Environment.DirectionType.DownLeft
						, _speed160_4538);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_154_4299 = false;
				}
			}
			if(!_matched_154_4299 || _fallthrough_154_4299) {
				if(Equals(_switch154_4299, 6)) {
					_matched_154_4299 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed161_4585 = 1
					;
						
						var _entity161_4585 = this;
						
						Func<double[], bool> _predicate161_4585 = null;
						
						_Battleground._greenEnvironment.MoveTowards(_entity161_4585, Mars.Interfaces.Environment.DirectionType.Left
						, _speed161_4585);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_154_4299 = false;
				}
			}
			if(!_matched_154_4299 || _fallthrough_154_4299) {
				if(Equals(_switch154_4299, 7)) {
					_matched_154_4299 = true;
					{
					new System.Func<Tuple<double,double>>(() => {
						
						var _speed162_4621 = 1
					;
						
						var _entity162_4621 = this;
						
						Func<double[], bool> _predicate162_4621 = null;
						
						_Battleground._greenEnvironment.MoveTowards(_entity162_4621, Mars.Interfaces.Environment.DirectionType.UpLeft
						, _speed162_4621);
						
						return new Tuple<double, double>(Position.X, Position.Y);
					}).Invoke()
					;}
				} else {
					_fallthrough_154_4299 = false;
				}
			}
			;}
			return;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public virtual void shoot() 
		{
			{
			}
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
		public Mars.Components.Environments.SpatialHashEnvironment<green> _greenEnvironment { get; set; }
		public Mars.Components.Environments.SpatialHashEnvironment<red> _redEnvironment { get; set; }
		public Mars.Components.Environments.SpatialHashEnvironment<blue> _blueEnvironment { get; set; }
		public Mars.Components.Environments.SpatialHashEnvironment<yellow> _yellowEnvironment { get; set; }
		
		[Mars.Interfaces.LIFECapabilities.PublishForMappingInMars]
		public green (
		System.Guid _id,
		lasertag.Battleground _layer,
		Mars.Interfaces.Layer.RegisterAgent _register,
		Mars.Interfaces.Layer.UnregisterAgent _unregister,
		Mars.Components.Environments.SpatialHashEnvironment<green> _greenEnvironment,
		double xcor = 0, double ycor = 0, int freq = 1)
		{
			_Battleground = _layer;
			ID = _id;
			Position = Mars.Interfaces.Environment.Position.CreatePosition(xcor, ycor);
			_Random = new System.Random(ID.GetHashCode());
			_Battleground._greenEnvironment.Insert(this);
			_register(_layer, this, freq);
			_isAlive = true;
			_executionFrequency = freq;
			{
			new System.Func<System.Tuple<double,double>>(() => {
				
				var _taget27_808 = new System.Tuple<int,int>(1,1);
				
				var _object27_808 = this;
				
				_Battleground._greenEnvironment.PosAt(_object27_808, 
					_taget27_808.Item1, _taget27_808.Item2
				);
				return new Tuple<double, double>(Position.X, Position.Y);
			}).Invoke()
			;}
		}
		
		public void Tick()
		{
			{ if (!_isAlive) return; }
			{
			explore_env();
			new System.Func<Tuple<double,double>>(() => {
				
				const int _speed41_1085 = 1
						;
				
				var _entity41_1085 = this;
				
				Func<double[], bool> _predicate41_1085 = null;
				
				_Battleground._greenEnvironment.MoveTowards(_entity41_1085, Mars.Interfaces.Environment.DirectionType.Right
				, _speed41_1085);
				
				return new Tuple<double, double>(Position.X, Position.Y);
			}).Invoke();
			refill_action_points()
			;}
		}
		
		public System.Guid ID { get; }
		public Mars.Interfaces.Environment.Position Position { get; set; }
		public bool Equals(green other) => Equals(ID, other.ID);
		public override int GetHashCode() => ID.GetHashCode();
	}
}

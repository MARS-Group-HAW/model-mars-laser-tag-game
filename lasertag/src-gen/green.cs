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
		private int __movementRange
			 = 6;
		public int movementRange { 
			get { return __movementRange; }
			set{
				if(__movementRange != value) __movementRange = value;
			}
		}
		private int __movementPoints
			 = 6;
		public int movementPoints { 
			get { return __movementPoints; }
			set{
				if(__movementPoints != value) __movementPoints = value;
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
		public void move_me(string direction) 
		{
			{
			if(movementPoints < 1) {
							{
							return 
							;}
					;} ;
			string _switch52_1631 = (direction);
			bool _matched_52_1631 = false;
			bool _fallthrough_52_1631 = false;
			if(!_matched_52_1631 || _fallthrough_52_1631) {
				if(Equals(_switch52_1631, "up")) {
					_matched_52_1631 = true;
					{
					if((Equals(battleground.GetIntegerValue(this.Position.X,this.Position.Y + 1)
					, 0))) {
									{
									battleground.SetIntegerValue(this.Position.X,this.Position.Y,0);
									new System.Func<Tuple<double,double>>(() => {
										
										var _speed56_1792 = 1
									;
										
										var _entity56_1792 = this;
										
										Func<double[], bool> _predicate56_1792 = null;
										
										_Battleground._greenEnvironment.MoveTowards(_entity56_1792, Mars.Interfaces.Environment.DirectionType.Up
										, _speed56_1792);
										
										return new Tuple<double, double>(Position.X, Position.Y);
									}).Invoke();
									battleground.SetIntegerValue(this.Position.X,this.Position.Y,2);
									movementPoints = movementPoints - 1
									;}
							;} 
					;}
				} else {
					_fallthrough_52_1631 = false;
				}
			}
			if(!_matched_52_1631 || _fallthrough_52_1631) {
				if(Equals(_switch52_1631, "up-right")) {
					_matched_52_1631 = true;
					{
					if((Equals(battleground.GetIntegerValue(this.Position.X + 1,this.Position.Y + 1)
					, 0))) {
									{
									battleground.SetIntegerValue(this.Position.X,this.Position.Y,0);
									new System.Func<Tuple<double,double>>(() => {
										
										var _speed64_2057 = 1
									;
										
										var _entity64_2057 = this;
										
										Func<double[], bool> _predicate64_2057 = null;
										
										_Battleground._greenEnvironment.MoveTowards(_entity64_2057, Mars.Interfaces.Environment.DirectionType.UpRight
										, _speed64_2057);
										
										return new Tuple<double, double>(Position.X, Position.Y);
									}).Invoke();
									battleground.SetIntegerValue(this.Position.X,this.Position.Y,2);
									movementPoints = movementPoints - 1
									;}
							;} 
					;}
				} else {
					_fallthrough_52_1631 = false;
				}
			}
			if(!_matched_52_1631 || _fallthrough_52_1631) {
				if(Equals(_switch52_1631, "right")) {
					_matched_52_1631 = true;
					{
					if((Equals(battleground.GetIntegerValue(this.Position.X + 1,this.Position.Y)
					, 0))) {
									{
									battleground.SetIntegerValue(this.Position.X,this.Position.Y,0);
									new System.Func<Tuple<double,double>>(() => {
										
										var _speed72_2321 = 1
									;
										
										var _entity72_2321 = this;
										
										Func<double[], bool> _predicate72_2321 = null;
										
										_Battleground._greenEnvironment.MoveTowards(_entity72_2321, Mars.Interfaces.Environment.DirectionType.Right
										, _speed72_2321);
										
										return new Tuple<double, double>(Position.X, Position.Y);
									}).Invoke();
									battleground.SetIntegerValue(this.Position.X,this.Position.Y,2);
									movementPoints = movementPoints - 1
									;}
							;} 
					;}
				} else {
					_fallthrough_52_1631 = false;
				}
			}
			if(!_matched_52_1631 || _fallthrough_52_1631) {
				if(Equals(_switch52_1631, "down-right")) {
					_matched_52_1631 = true;
					{
					if((Equals(battleground.GetIntegerValue(this.Position.X + 1,this.Position.Y - 1)
					, 0))) {
									{
									battleground.SetIntegerValue(this.Position.X,this.Position.Y,0);
									new System.Func<Tuple<double,double>>(() => {
										
										var _speed80_2591 = 1
									;
										
										var _entity80_2591 = this;
										
										Func<double[], bool> _predicate80_2591 = null;
										
										_Battleground._greenEnvironment.MoveTowards(_entity80_2591, Mars.Interfaces.Environment.DirectionType.DownRight
										, _speed80_2591);
										
										return new Tuple<double, double>(Position.X, Position.Y);
									}).Invoke();
									battleground.SetIntegerValue(this.Position.X,this.Position.Y,2);
									movementPoints = movementPoints - 1
									;}
							;} 
					;}
				} else {
					_fallthrough_52_1631 = false;
				}
			}
			if(!_matched_52_1631 || _fallthrough_52_1631) {
				if(Equals(_switch52_1631, "down")) {
					_matched_52_1631 = true;
					{
					if((Equals(battleground.GetIntegerValue(this.Position.X,this.Position.Y - 1)
					, 0))) {
									{
									battleground.SetIntegerValue(this.Position.X,this.Position.Y,0);
									new System.Func<Tuple<double,double>>(() => {
										
										var _speed88_2856 = 1
									;
										
										var _entity88_2856 = this;
										
										Func<double[], bool> _predicate88_2856 = null;
										
										_Battleground._greenEnvironment.MoveTowards(_entity88_2856, Mars.Interfaces.Environment.DirectionType.Down
										, _speed88_2856);
										
										return new Tuple<double, double>(Position.X, Position.Y);
									}).Invoke();
									battleground.SetIntegerValue(this.Position.X,this.Position.Y,2);
									movementPoints = movementPoints - 1
									;}
							;} 
					;}
				} else {
					_fallthrough_52_1631 = false;
				}
			}
			if(!_matched_52_1631 || _fallthrough_52_1631) {
				if(Equals(_switch52_1631, "down-left")) {
					_matched_52_1631 = true;
					{
					if((Equals(battleground.GetIntegerValue(this.Position.X - 1,this.Position.Y - 1)
					, 0))) {
									{
									battleground.SetIntegerValue(this.Position.X,this.Position.Y,0);
									new System.Func<Tuple<double,double>>(() => {
										
										var _speed96_3124 = 1
									;
										
										var _entity96_3124 = this;
										
										Func<double[], bool> _predicate96_3124 = null;
										
										_Battleground._greenEnvironment.MoveTowards(_entity96_3124, Mars.Interfaces.Environment.DirectionType.DownLeft
										, _speed96_3124);
										
										return new Tuple<double, double>(Position.X, Position.Y);
									}).Invoke();
									battleground.SetIntegerValue(this.Position.X,this.Position.Y,2);
									movementPoints = movementPoints - 1
									;}
							;} 
					;}
				} else {
					_fallthrough_52_1631 = false;
				}
			}
			if(!_matched_52_1631 || _fallthrough_52_1631) {
				if(Equals(_switch52_1631, "left")) {
					_matched_52_1631 = true;
					{
					if((Equals(battleground.GetIntegerValue(this.Position.X - 1,this.Position.Y)
					, 0))) {
									{
									battleground.SetIntegerValue(this.Position.X,this.Position.Y,0);
									new System.Func<Tuple<double,double>>(() => {
										
										var _speed104_3388 = 1
									;
										
										var _entity104_3388 = this;
										
										Func<double[], bool> _predicate104_3388 = null;
										
										_Battleground._greenEnvironment.MoveTowards(_entity104_3388, Mars.Interfaces.Environment.DirectionType.Left
										, _speed104_3388);
										
										return new Tuple<double, double>(Position.X, Position.Y);
									}).Invoke();
									battleground.SetIntegerValue(this.Position.X,this.Position.Y,2);
									movementPoints = movementPoints - 1
									;}
							;} 
					;}
				} else {
					_fallthrough_52_1631 = false;
				}
			}
			if(!_matched_52_1631 || _fallthrough_52_1631) {
				if(Equals(_switch52_1631, "up-left")) {
					_matched_52_1631 = true;
					{
					if((Equals(battleground.GetIntegerValue(this.Position.X - 1,this.Position.Y + 1)
					, 0))) {
									{
									battleground.SetIntegerValue(this.Position.X,this.Position.Y,0);
									new System.Func<Tuple<double,double>>(() => {
										
										var _speed112_3654 = 1
									;
										
										var _entity112_3654 = this;
										
										Func<double[], bool> _predicate112_3654 = null;
										
										_Battleground._greenEnvironment.MoveTowards(_entity112_3654, Mars.Interfaces.Environment.DirectionType.UpLeft
										, _speed112_3654);
										
										return new Tuple<double, double>(Position.X, Position.Y);
									}).Invoke();
									battleground.SetIntegerValue(this.Position.X,this.Position.Y,2);
									movementPoints = movementPoints - 1
									;}
							;} 
					;}
				} else {
					_fallthrough_52_1631 = false;
				}
			}
			;}
			return;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void refill_points() 
		{
			{
			actionPoints = 10;
			movementPoints = movementRange
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
			actionPoints = actionPoints - 2;
			lasertag.stance _switch137_4363 = (currStance);
			bool _matched_137_4363 = false;
			bool _fallthrough_137_4363 = false;
			if(!_matched_137_4363 || _fallthrough_137_4363) {
				if(Equals(_switch137_4363, stance.Standing)) {
					_matched_137_4363 = true;
					{
					visualRange = 10;
					visibility = 10;
					movementRange = 6
					;}
				} else {
					_fallthrough_137_4363 = false;
				}
			}
			if(!_matched_137_4363 || _fallthrough_137_4363) {
				if(Equals(_switch137_4363, stance.Kneeling)) {
					_matched_137_4363 = true;
					{
					visualRange = 8;
					visibility = 8;
					movementRange = 3
					;}
				} else {
					_fallthrough_137_4363 = false;
				}
			}
			if(!_matched_137_4363 || _fallthrough_137_4363) {
				if(Equals(_switch137_4363, stance.Lying)) {
					_matched_137_4363 = true;
					{
					visualRange = 5;
					visibility = 5;
					movementRange = 1
					;}
				} else {
					_fallthrough_137_4363 = false;
				}
			}
			;}
			return;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void explore_env() 
		{
			{
			actionPoints = actionPoints - 1;
			explore_team();
			explore_walls()
			;}
			return;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void explore_team() 
		{
			{
			teamList = new System.Func<lasertag.green[]>(() => {
				
				var _sourceMapped166_5006 = this.Position;
				var _source166_5006 = _sourceMapped166_5006;
				var _range166_5006 = -1;
							
				Func<lasertag.green, bool> _predicate166_5006 = null;
				Func<lasertag.green, bool> _predicateMod166_5006 = new Func<lasertag.green, bool>(_it => 
				{
					if (_it?.ID == this.ID)
					{
						return false;
					} else if (_predicate166_5006 != null)
					{
						return _predicate166_5006.Invoke(_it);
					} else return true;
				});
				
				return _Battleground._greenEnvironment.Explore(_source166_5006 , _range166_5006, -1, _predicate166_5006).ToArray();
			}).Invoke()
			;}
			return;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void explore_walls() 
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
		public void explore_enemies() 
		{
			{
			lasertag.red[] tmpRed = new System.Func<lasertag.red[]>(() => {
				
				var _sourceMapped191_6038 = this.Position;
				var _source191_6038 = _sourceMapped191_6038;
				var _range191_6038 = -1;
							
				Func<lasertag.red, bool> _predicate191_6038 = new Func<lasertag.red,bool>((lasertag.red x) => 
				 {
						{
						return (new Func<double>(() => {
						var _target191_6070 = x;
						return Mars.Mathematics.Distance.Calculate(Mars.Mathematics.SpaceDistanceMetric.Euclidean, this.Position.PositionArray, _target191_6070.Position.PositionArray);
						}).Invoke()) <= visualRange && (new Func<double>(() => {
						var _target191_6102 = x;
						return Mars.Mathematics.Distance.Calculate(Mars.Mathematics.SpaceDistanceMetric.Euclidean, this.Position.PositionArray, _target191_6102.Position.PositionArray);
						}).Invoke()) <= x.GetVisibility()
						;}
						;
						return default(bool);;
				});
				Func<lasertag.red, bool> _predicateMod191_6038 = new Func<lasertag.red, bool>(_it => 
				{
					if (_it?.ID == this.ID)
					{
						return false;
					} else if (_predicate191_6038 != null)
					{
						return _predicate191_6038.Invoke(_it);
					} else return true;
				});
				
				return _Battleground._redEnvironment.Explore(_source191_6038 , _range191_6038, -1, _predicate191_6038).ToArray();
			}).Invoke();
			lasertag.blue[] tmpBlue = new System.Func<lasertag.blue[]>(() => {
				
				var _sourceMapped192_6151 = this.Position;
				var _source192_6151 = _sourceMapped192_6151;
				var _range192_6151 = -1;
							
				Func<lasertag.blue, bool> _predicate192_6151 = new Func<lasertag.blue,bool>((lasertag.blue x) => 
				 {
						{
						return (new Func<double>(() => {
						var _target192_6184 = x;
						return Mars.Mathematics.Distance.Calculate(Mars.Mathematics.SpaceDistanceMetric.Euclidean, this.Position.PositionArray, _target192_6184.Position.PositionArray);
						}).Invoke()) <= visualRange && (new Func<double>(() => {
						var _target192_6216 = x;
						return Mars.Mathematics.Distance.Calculate(Mars.Mathematics.SpaceDistanceMetric.Euclidean, this.Position.PositionArray, _target192_6216.Position.PositionArray);
						}).Invoke()) <= x.GetVisibility()
						;}
						;
						return default(bool);;
				});
				Func<lasertag.blue, bool> _predicateMod192_6151 = new Func<lasertag.blue, bool>(_it => 
				{
					if (_it?.ID == this.ID)
					{
						return false;
					} else if (_predicate192_6151 != null)
					{
						return _predicate192_6151.Invoke(_it);
					} else return true;
				});
				
				return _Battleground._blueEnvironment.Explore(_source192_6151 , _range192_6151, -1, _predicate192_6151).ToArray();
			}).Invoke();
			lasertag.yellow[] tmpYellow = new System.Func<lasertag.yellow[]>(() => {
				
				var _sourceMapped193_6267 = this.Position;
				var _source193_6267 = _sourceMapped193_6267;
				var _range193_6267 = -1;
							
				Func<lasertag.yellow, bool> _predicate193_6267 = new Func<lasertag.yellow,bool>((lasertag.yellow x) => 
				 {
						{
						return (new Func<double>(() => {
						var _target193_6302 = x;
						return Mars.Mathematics.Distance.Calculate(Mars.Mathematics.SpaceDistanceMetric.Euclidean, this.Position.PositionArray, _target193_6302.Position.PositionArray);
						}).Invoke()) <= visualRange && (new Func<double>(() => {
						var _target193_6334 = x;
						return Mars.Mathematics.Distance.Calculate(Mars.Mathematics.SpaceDistanceMetric.Euclidean, this.Position.PositionArray, _target193_6334.Position.PositionArray);
						}).Invoke()) <= x.GetVisibility()
						;}
						;
						return default(bool);;
				});
				Func<lasertag.yellow, bool> _predicateMod193_6267 = new Func<lasertag.yellow, bool>(_it => 
				{
					if (_it?.ID == this.ID)
					{
						return false;
					} else if (_predicate193_6267 != null)
					{
						return _predicate193_6267.Invoke(_it);
					} else return true;
				});
				
				return _Battleground._yellowEnvironment.Explore(_source193_6267 , _range193_6267, -1, _predicate193_6267).ToArray();
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
														if(true) {
																		{
																		}
																;} else {
																		{
																		}
																	;}
													;}
									;}
						;}
					};
			enemyList = new System.Tuple<Mars.Components.Common.MarsList<lasertag.red>,lasertag.blue[],lasertag.yellow[]>(redList,tmpBlue,tmpYellow)
			;}
			return;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void tag() 
		{
			{
			}
			return;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void reload() 
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
				
				var _taget32_1033 = new System.Tuple<int,int>(1,4);
				
				var _object32_1033 = this;
				
				_Battleground._greenEnvironment.PosAt(_object32_1033, 
					_taget32_1033.Item1, _taget32_1033.Item2
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
			move_me("up");
			refill_points()
			;}
		}
		
		public System.Guid ID { get; }
		public Mars.Interfaces.Environment.Position Position { get; set; }
		public bool Equals(green other) => Equals(ID, other.ID);
		public override int GetHashCode() => ID.GetHashCode();
	}
}

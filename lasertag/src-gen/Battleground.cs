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
	public class Battleground : Mars.Components.Layers.AbstractLayer {
		private static readonly Mars.Common.Logging.ILogger _Logger = 
					Mars.Common.Logging.LoggerFactory.GetLogger(typeof(Battleground));
		private readonly System.Random _Random = new System.Random();
		public Mars.Interfaces.Layer.UnregisterAgent _Unregister { get; set; }
		public Mars.Interfaces.Layer.RegisterAgent _Register { get; set; }
		public Mars.Mathematics.SpaceDistanceMetric _DistanceMetric { get; set; } = Mars.Mathematics.SpaceDistanceMetric.Euclidean;
		private int _dimensionX, _dimensionY;
		public int DimensionX() => _dimensionX;
		public int DimensionY() => _dimensionY;
		public System.Collections.Concurrent.ConcurrentDictionary<Mars.Interfaces.Environment.Position, string> 
			_StringBattleground { get; set; }
		public System.Collections.Concurrent.ConcurrentDictionary<Mars.Interfaces.Environment.Position, double> 
			_RealBattleground { get; set; }
		
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public string GetStringValue(int x, int y) => GetStringValue((double)x, y);
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public string GetStringValue(double x, double y) =>
			_StringBattleground.TryGetValue(Mars.Interfaces.Environment.Position.CreatePosition(x, y), out var value) ? value : null;
		
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public int GetIntegerValue(int x, int y) => GetIntegerValue((double)x, y);
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public int GetIntegerValue(double x, double y) =>
					_RealBattleground.TryGetValue(Mars.Interfaces.Environment.Position.CreatePosition(x, y), out var value) ? Convert.ToInt32(value) : 0;
		
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public double GetRealValue(int x, int y) => GetRealValue((double)x, y);
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public double GetRealValue(double x, double y) =>
			_RealBattleground.TryGetValue(Mars.Interfaces.Environment.Position.CreatePosition(x, y), out var value) ? value : 0;
		
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void SetStringValue(int x, int y, string value) => SetStringValue((double)x, y, value);
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void SetStringValue(double x, double y, string value)
		{
			if (value != null)
				_StringBattleground.AddOrUpdate(Mars.Interfaces.Environment.Position.CreatePosition(x, y), value,
					(position, s) => value);
		}
		
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void SetIntegerValue(double x, double y, int value) => SetRealValue(x, y, value);
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void SetIntegerValue(int x, int y, int value) => SetRealValue((double)x, y, value);
		
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void SetRealValue(int x, int y, double value) => SetRealValue((double)x, y, value);
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void SetRealValue(double x, double y, double value)
		{
			if (Math.Abs(value) > 0.000000001)
				_RealBattleground.AddOrUpdate(Mars.Interfaces.Environment.Position.CreatePosition(x, y), value,
					(position, s) => value);
		}
		
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private void _InitGrid(Mars.Interfaces.Layer.Initialization.TInitData initData)
		{
			if (initData.LayerInitConfig != null && !string.IsNullOrEmpty(initData.LayerInitConfig.File))
			{
				var table = Mars.Common.IO.Csv.CsvReader.MapData(initData.LayerInitConfig.File, null, false);
				
				var xMaxIndex = table.Columns.Count;
				int yMaxIndex = table.Rows.Count - 1;
		
				_dimensionX = table.Columns.Count;
				_dimensionY = table.Rows.Count;
				foreach (System.Data.DataRow tableRow in table.Rows)
				{
					for (int x = 0; x < xMaxIndex; x++)
					{
						var value = tableRow[x].ToString();
						if (double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture,
							out var result))
							SetRealValue(x, yMaxIndex, result);
						else
							SetStringValue(x, yMaxIndex, value);
					}
					yMaxIndex --;
				}
			}
		}
		
		public Mars.Components.Environments.SpatialHashEnvironment<blue> _blueEnvironment { get; set; }
		public Mars.Components.Environments.SpatialHashEnvironment<green> _greenEnvironment { get; set; }
		public Mars.Components.Environments.SpatialHashEnvironment<red> _redEnvironment { get; set; }
		public Mars.Components.Environments.SpatialHashEnvironment<yellow> _yellowEnvironment { get; set; }
		public System.Collections.Generic.IDictionary<System.Guid, blue> _blueAgents { get; set; }
		public System.Collections.Generic.IDictionary<System.Guid, green> _greenAgents { get; set; }
		public System.Collections.Generic.IDictionary<System.Guid, red> _redAgents { get; set; }
		public System.Collections.Generic.IDictionary<System.Guid, yellow> _yellowAgents { get; set; }
		
		public Battleground _Battleground => this;
		public Battleground battleground => this;
		public Battleground (
		double dimensionX = 100, double dimensionY = 100, int cellSize = 1
				) {
			_dimensionX = Convert.ToInt32(dimensionX); _dimensionY = Convert.ToInt32(dimensionY);
			_StringBattleground = new System.Collections.Concurrent.ConcurrentDictionary<Mars.Interfaces.Environment.Position, string>();
			_RealBattleground = new System.Collections.Concurrent.ConcurrentDictionary<Mars.Interfaces.Environment.Position, double>();
		}
		
		public override bool InitLayer(
			Mars.Interfaces.Layer.Initialization.TInitData initData, 
			Mars.Interfaces.Layer.RegisterAgent regHandle, 
			Mars.Interfaces.Layer.UnregisterAgent unregHandle)
		{
			base.InitLayer(initData, regHandle, unregHandle);
			this._Register = regHandle;
			this._Unregister = unregHandle;
			
			_InitGrid(initData);
			this._blueEnvironment = new Mars.Components.Environments.SpatialHashEnvironment<blue>(_dimensionX, _dimensionY, true);
			this._greenEnvironment = new Mars.Components.Environments.SpatialHashEnvironment<green>(_dimensionX, _dimensionY, true);
			this._redEnvironment = new Mars.Components.Environments.SpatialHashEnvironment<red>(_dimensionX, _dimensionY, true);
			this._yellowEnvironment = new Mars.Components.Environments.SpatialHashEnvironment<yellow>(_dimensionX, _dimensionY, true);
			
			_blueAgents = Mars.Components.Services.AgentManager.SpawnAgents<blue>(
			initData.AgentInitConfigs.First(config => config.Type == typeof(blue)),
			regHandle, unregHandle, 
			new System.Collections.Generic.List<Mars.Interfaces.Layer.ILayer> { this });
			_greenAgents = Mars.Components.Services.AgentManager.SpawnAgents<green>(
			initData.AgentInitConfigs.First(config => config.Type == typeof(green)),
			regHandle, unregHandle, 
			new System.Collections.Generic.List<Mars.Interfaces.Layer.ILayer> { this });
			_redAgents = Mars.Components.Services.AgentManager.SpawnAgents<red>(
			initData.AgentInitConfigs.First(config => config.Type == typeof(red)),
			regHandle, unregHandle, 
			new System.Collections.Generic.List<Mars.Interfaces.Layer.ILayer> { this });
			_yellowAgents = Mars.Components.Services.AgentManager.SpawnAgents<yellow>(
			initData.AgentInitConfigs.First(config => config.Type == typeof(yellow)),
			regHandle, unregHandle, 
			new System.Collections.Generic.List<Mars.Interfaces.Layer.ILayer> { this });
			
			return true;
		}
		
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public lasertag.blue _Spawnblue(double xcor = 0, double ycor = 0, int freq = 1) {
			var id = System.Guid.NewGuid();
			var agent = new lasertag.blue(id, this, _Register, _Unregister,
			_blueEnvironment,
			xcor, ycor, freq);
			_blueAgents.Add(id, agent);
			return agent;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public lasertag.green _Spawngreen(double xcor = 0, double ycor = 0, int freq = 1) {
			var id = System.Guid.NewGuid();
			var agent = new lasertag.green(id, this, _Register, _Unregister,
			_greenEnvironment,
			xcor, ycor, freq);
			_greenAgents.Add(id, agent);
			return agent;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public lasertag.red _Spawnred(double xcor = 0, double ycor = 0, int freq = 1) {
			var id = System.Guid.NewGuid();
			var agent = new lasertag.red(id, this, _Register, _Unregister,
			_redEnvironment,
			xcor, ycor, freq);
			_redAgents.Add(id, agent);
			return agent;
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public lasertag.yellow _Spawnyellow(double xcor = 0, double ycor = 0, int freq = 1) {
			var id = System.Guid.NewGuid();
			var agent = new lasertag.yellow(id, this, _Register, _Unregister,
			_yellowEnvironment,
			xcor, ycor, freq);
			_yellowAgents.Add(id, agent);
			return agent;
		}
		
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void _Killblue(lasertag.blue target, int executionFrequency = 1)
		{
			target._isAlive = false;
			_blueEnvironment.Remove(target);
			_Unregister(this, target, target._executionFrequency);
			_blueAgents.Remove(target.ID);
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void _Killgreen(lasertag.green target, int executionFrequency = 1)
		{
			target._isAlive = false;
			_greenEnvironment.Remove(target);
			_Unregister(this, target, target._executionFrequency);
			_greenAgents.Remove(target.ID);
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void _Killred(lasertag.red target, int executionFrequency = 1)
		{
			target._isAlive = false;
			_redEnvironment.Remove(target);
			_Unregister(this, target, target._executionFrequency);
			_redAgents.Remove(target.ID);
		}
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void _Killyellow(lasertag.yellow target, int executionFrequency = 1)
		{
			target._isAlive = false;
			_yellowEnvironment.Remove(target);
			_Unregister(this, target, target._executionFrequency);
			_yellowAgents.Remove(target.ID);
		}
	}
}

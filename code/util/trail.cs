using System.Collections.Generic;
using Sandbox;
using System;

namespace Core
{
	public class NodeData
	{
		public Vector3 Position;
		public Vector3 Direction;
		public float Girth = 5f;
		public float EndGirth = 1f;
		public float LifeTime = 1f;
		public float Alpha = 255f;
		public float MaxLength = 10f;
		public bool IsDashed = false;
		public bool ShouldRemove = false;
		public float InterpolationMultiplier = 1;
		public Vector3 VertexPositionA;
		public Vector3 VertexPositionB;
		public Color NodeColor = new Color(255,255,255,255);
		public Interp<List<float>> Interpolation;

		public NodeData(float start_alpha, float end_alpha, float start_girth, float end_girth, float duration)
		{
			var start_values = new List<float>() {
				start_alpha,
				start_girth
			};
			var end_values = new List<float>() {
				end_alpha,
				end_girth
			};

			EndGirth = end_girth;
			Interpolation = new Interp<List<float>>(start_values, end_values, duration, 0, InterpFunctions.Linear);
			Interpolation.Enabled = true;
		}

		public void SetLifeTime(float life_time)
		{
			LifeTime = life_time;
			Interpolation.Duration = LifeTime;
		}

		public void UpdateVertexPosition()
		{
			Vector3 end_size = ((Local.Client.Pawn.EyePos - Position).Cross(Direction)).Normal;
			end_size *= (Girth/2);

			VertexPositionA = Position + (end_size);
			VertexPositionB = Position - (end_size);
		}

		public void Update()
		{
				var interp_value = Interpolation.GetValue();

				if ((Alpha == 0 || Girth == EndGirth) || Interpolation.CurrentTime == Interpolation.Duration)
					ShouldRemove = true;

				Alpha = interp_value[0];
				NodeColor.a = Alpha/255;
				Girth = interp_value[1];
				UpdateVertexPosition();
				Interpolation.Update(Time.Delta * InterpolationMultiplier);
		}
	}

	public partial class Trail : RenderEntity
	{
		public List<NodeData> Nodes = new List<NodeData>();
		public bool DrawTempSegment = true;
		public bool IsDashed = false;
		public int MaxNodes = 500;
		public int MaxNodesPerBuffer = 100;
		public float StartGirth = 5;
		public Material Mat = Material.Load("materials/tools/vertex_color_translucent.vmat");
		public Color TrailColor = new Color(255, 255, 255, 255);

		public void DrawTrailSegment(VertexBuffer vertex_buffer, NodeData end_node, NodeData start_node) // Segments all trail segments and connects them at the closest corner
		{
			Vertex vertex_top_right;
			Vertex vertex_top_left;
			Vertex vertex_bottom_right;
			Vertex vertex_bottom_left;
			Vector3 start_size = ((Local.Client.Pawn.EyePos - start_node.Position).Cross(end_node.Position - start_node.Position)).Normal * start_node.Girth;

			if ((end_node.VertexPositionA - start_node.VertexPositionA).LengthSquared > (end_node.VertexPositionB - start_node.VertexPositionB).LengthSquared)
			{
				vertex_top_right = new Vertex(start_node.VertexPositionB + start_size, new Vector4(), start_node.NodeColor);
				vertex_top_left = new Vertex(start_node.VertexPositionB, new Vector4(), start_node.NodeColor);
			}
			else
			{
				vertex_top_right = new Vertex(start_node.VertexPositionA, new Vector4(), start_node.NodeColor);
				vertex_top_left = new Vertex(start_node.VertexPositionA - start_size, new Vector4(), start_node.NodeColor);
			}

			vertex_bottom_right = new Vertex(end_node.VertexPositionA, new Vector4(), end_node.NodeColor);
			vertex_bottom_left = new Vertex(end_node.VertexPositionB, new Vector4(), end_node.NodeColor);
			vertex_buffer.AddQuad(vertex_top_right, vertex_top_left, vertex_bottom_left, vertex_bottom_right);
		}

		public void DrawTrailAttached(VertexBuffer vertex_buffer, NodeData end_node, NodeData start_node)
		{
			Vertex vertex_top_right = new Vertex(start_node.VertexPositionA, new Vector4(), start_node.NodeColor);
			Vertex vertex_top_left = new Vertex(start_node.VertexPositionB, new Vector4(), start_node.NodeColor);
			Vertex vertex_bottom_right = new Vertex(end_node.VertexPositionA, new Vector4(), end_node.NodeColor);
			Vertex vertex_bottom_left = new Vertex(end_node.VertexPositionB, new Vector4(), end_node.NodeColor);
			
			vertex_buffer.AddQuad(vertex_top_right, vertex_top_left, vertex_bottom_left, vertex_bottom_right);
		}

		public void DrawTrailSegment(VertexBuffer vertex_buffer, NodeData start_node, Vector3 end_position, float end_girth, Color end_color)
		{
			Vector3 start_position = start_node.Position;
			Vector3 end_size = ((Local.Client.Pawn.EyePos - end_position).Cross(end_position - start_position)).Normal;

			end_size *= (end_girth/2);

			Vertex vertex_top_right = new Vertex(start_node.VertexPositionA, new Vector4(), start_node.NodeColor);
			Vertex vertex_top_left = new Vertex(start_node.VertexPositionB, new Vector4(), start_node.NodeColor);
			Vertex vertex_bottom_right = new Vertex(end_position + end_size, new Vector4(), end_color);
			Vertex vertex_bottom_left = new Vertex(end_position - end_size, new Vector4(), end_color);

			vertex_buffer.AddQuad(vertex_top_right, vertex_top_left, vertex_bottom_left, vertex_bottom_right);
		}

		public void DrawTrail()
		{
			VertexBuffer vertex_buffer = new VertexBuffer();
			int node_count = Nodes.Count;

			vertex_buffer.Init(true);

			for (int i = 1; i < node_count; i++)
			{
				DrawTrailAttached(vertex_buffer, Nodes[i], Nodes[i - 1]);

				if (i % MaxNodesPerBuffer == 0) // Create a new buffer and draw the current one
				{
					vertex_buffer.Draw(Mat);
					vertex_buffer = new VertexBuffer();
				}
			}
			
			if (node_count > 0)
			{
				if (DrawTempSegment)
					DrawTrailSegment(vertex_buffer, Nodes[node_count - 1], this.Position, Nodes[node_count - 1].Girth, TrailColor);

				vertex_buffer.Draw(Mat);
			}
		}

		// Speed up the decay of a node
		public void SafeRemoveNode(int node_position, float delta_multiplier = 20f)
		{
			Nodes[node_position].InterpolationMultiplier = delta_multiplier;
		}

		public void UpdateNode()
		{
			Vector3 mins = this.Position;
			Vector3 maxs = this.Position;
			int node_count = Nodes.Count;

			for (int i = 0; i < node_count; i++)
			{
				var node = Nodes[i];

				if (node.ShouldRemove)
				{
					Nodes.RemoveAt(i);
					i -= 1;
					node_count -= 1;
				}
				else
				{
					// Handle BBox
					var position = Nodes[i].Position;
					var girth  = new Vector3(Nodes[i].Girth);
					var node_mins = position - girth;
					var node_maxs = position + girth;

					Nodes[i].Update();
					mins = new Vector3(MathF.Min(mins.x, node_mins.x), MathF.Min(mins.y, node_mins.y), MathF.Min(mins.z, node_mins.z));
					maxs = new Vector3(MathF.Max(maxs.x, node_maxs.x), MathF.Max(maxs.y, node_maxs.y), MathF.Max(maxs.z, node_maxs.z));
				}
			}
			
			if (this.Parent != null)
			{
				Position = this.Parent.Position + new Vector3(0, 0, StartGirth);
				Velocity = this.Parent.Velocity;
			}

			RenderBounds = new BBox(mins - this.Position, maxs - this.Position);
		}

		public bool NewNode(object pos = null, float end_girth = 0f, float life_time = 5f, float alpha = 255f, float max_length = 5f, bool is_dashed = false)
		{
			if (Velocity.LengthSquared > 0)
			{
				int node_count = Nodes.Count;
				Vector3 position = (pos == null) ? this.Position : (Vector3)pos;

				if (node_count > 0)
				{
					var previous_node = Nodes[node_count - 1];

					if (previous_node.Position != position && (previous_node.Position - position).LengthSquared >= (max_length * max_length))
					{
						var node = new NodeData(alpha, 0, StartGirth, end_girth, life_time);
						node.Position = position;
						node.Direction = (node.Position - previous_node.Position).Normal;
						node.MaxLength = max_length;
						node.NodeColor = TrailColor;
						node.IsDashed = (IsDashed && !previous_node.IsDashed ? true : false);
						Nodes.Add(node);

						if (node_count >= MaxNodes)
						{
							for (int i = 0; i < (node_count - MaxNodes); i++)
							{
								SafeRemoveNode(i);
							}
						}

						return true;
					}
				}
				else
				{
					var node = new NodeData(alpha, 0, StartGirth, end_girth, life_time);
					node.Position = position;
					node.Direction = Velocity.Normal;
					node.MaxLength = max_length;
					node.NodeColor = TrailColor;
					node.IsDashed = IsDashed;
					Nodes.Add(node);

					return true;
				}
			}

			return false;
		}

		public override void DoRender(SceneObject obj)
		{
			DrawTrail();
		}
	}
}

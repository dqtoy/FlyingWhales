using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PathFind {
	public static class PathFind {
		public static Path<Node> FindPath<Node>(Node start, Node destination, Func<Node, Node, double> distance, Func<Node, double> estimate, PATHFINDING_MODE pathfindingMode, Kingdom kingdom = null) 
			where Node : HexTile, IHasNeighbours<Node> {

			var closed = new HashSet<Node>();
			var queue = new PriorityQueue<double, Path<Node>>();
			queue.Enqueue(0, new Path<Node>(start));
			Node lastStep = start;
			while (!queue.IsEmpty) {
				var path = queue.Dequeue();
				if (closed.Contains(path.LastStep))
					continue;
				if (path.LastStep.Equals(destination))
					return path;

				closed.Add(path.LastStep);
				lastStep = path.LastStep;


				double d;
				Path<Node> newPath;
                if (pathfindingMode == PATHFINDING_MODE.RESOURCE_PRODUCTION) {
                    foreach (Node n in path.LastStep.PurchasableTiles) {
                        if (n.tileTag != start.tileTag) {
                            continue;
                        }
                        if (n.isOccupied) {
                            if (!start.city.ownedTiles.Contains(n)) {
                                continue;
                            }
                        }
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.COMBAT) {
                    foreach (Node n in path.LastStep.CombatTiles) {
                        if (n.tileTag != start.tileTag) {
                            continue;
                        }
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.USE_ROADS) {
                    foreach (Node n in path.LastStep.RoadTiles) {
                        if (n.tileTag != start.tileTag) {
                            continue;
                        }
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.AVATAR) {
                    foreach (Node n in path.LastStep.AvatarTiles) {
                        if (n.tileTag != start.tileTag) {
                            continue;
                        }
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else if (pathfindingMode == PATHFINDING_MODE.NO_HIDDEN_TILES) {
                    if(kingdom == null) {
                        throw new Exception("Someone is trying to pathfind using NO_HIDDEN_TILES, but hasn't specified a kingdom!");
                    }
                    foreach (Node n in path.LastStep.AvatarTiles) {
                        if (n.tileTag != start.tileTag) {
                            continue;
                        }
                        if (kingdom.fogOfWarDict[FOG_OF_WAR_STATE.HIDDEN].Contains(n)) {
                            continue;
                        }
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                } else {
                    foreach (Node n in path.LastStep.ValidTiles) {
                        if (n.tileTag != start.tileTag) {
                            continue;
                        }
                        d = distance(path.LastStep, n);
                        newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                    }
                }
			}
			return null;
		}
	}




}
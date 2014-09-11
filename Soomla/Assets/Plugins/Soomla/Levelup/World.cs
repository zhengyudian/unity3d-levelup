/// Copyright (C) 2012-2014 Soomla Inc.
///
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///      http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Soomla;

namespace Soomla.Levelup {

	public class World : SoomlaEntity<World> {
		private static string TAG = "SOOMLA World";

		public Gate Gate;
		public Dictionary<string, World> InnerWorldsMap = new Dictionary<string, World>();
		public IEnumerable<World> InnerWorldsList {
			get { return InnerWorldsMap.Values; }
		}
		public Dictionary<string, Score> Scores = new Dictionary<string, Score>();
		public List<Mission> Missions = new List<Mission>();

		public World(String id)
			: base(id)
		{
		}

		public World(string id, Gate gate, Dictionary<string, World> innerWorlds, Dictionary<string, Score> scores, List<Mission> missions)
			: base(id)
		{
			this.InnerWorldsMap = (innerWorlds != null) ? innerWorlds : new Dictionary<string, World>();
			this.Scores = (scores != null) ? scores : new Dictionary<string, Score>();
			this.Gate = gate;
			this.Missions = (missions != null) ? missions : new List<Mission>();
		}

		public World(JSONObject jsonWorld)
			: base(jsonWorld)
		{
			InnerWorldsMap = new Dictionary<string, World>();
			List<JSONObject> worldsJSON = jsonWorld[LUJSONConsts.LU_WORLDS].list;

			// Iterate over all inner worlds in the JSON array and for each one create
			// an instance according to the world type
			foreach (JSONObject worldJSON in worldsJSON) {
				World innerWorld = World.fromJSONObject(worldJSON);
				if (innerWorld != null) {
					InnerWorldsMap.Add(innerWorld._id, innerWorld);
				}
			}

			Scores = new Dictionary<String, Score>();
			List<JSONObject> scoresJSON = jsonWorld[LUJSONConsts.LU_SCORES].list;

			// Iterate over all scores in the JSON array and for each one create
			// an instance according to the score type
			foreach (JSONObject scoreJSON in scoresJSON) {
				Score score = Score.fromJSONObject(scoreJSON);
				if (score != null) {
					Scores.Add(score.ID, score);
				}
			}

			Missions = new List<Mission>();
			List<JSONObject> missionsJSON = jsonWorld[LUJSONConsts.LU_MISSIONS].list;

			// Iterate over all challenges in the JSON array and create an instance for each one
			foreach (JSONObject missionJSON in missionsJSON) {
				Missions.Add(Mission.fromJSONObject(missionJSON));
			}

			JSONObject gateJSON = jsonWorld[LUJSONConsts.LU_GATE];
			if (gateJSON != null && gateJSON.keys != null && gateJSON.keys.Count > 0) {
				Gate = Gate.fromJSONObject (gateJSON);
			}
		}

		public override JSONObject toJSONObject() {
			JSONObject obj = base.toJSONObject();

			obj.AddField(LUJSONConsts.LU_GATE, (Gate==null ? new JSONObject(JSONObject.Type.OBJECT) : Gate.toJSONObject()));

			JSONObject worldsArr = new JSONObject(JSONObject.Type.ARRAY);
			foreach (World world in InnerWorldsMap.Values) {
				worldsArr.Add(world.toJSONObject());
			}
			obj.AddField(LUJSONConsts.LU_WORLDS, worldsArr);

			JSONObject scoresArr = new JSONObject(JSONObject.Type.ARRAY);
			foreach (Score score in Scores.Values) {
				scoresArr.Add(score.toJSONObject());
			}
			obj.AddField(LUJSONConsts.LU_SCORES, scoresArr);

			JSONObject missionsArr = new JSONObject(JSONObject.Type.ARRAY);
			foreach (Mission mission in Missions) {
				missionsArr.Add(mission.toJSONObject());
			}
			obj.AddField(LUJSONConsts.LU_MISSIONS, missionsArr);

			return obj;
		}

		public static World fromJSONObject(JSONObject worldObj) {
			string className = worldObj[JSONConsts.SOOM_CLASSNAME].str;

			World world = (World) Activator.CreateInstance(Type.GetType("Soomla.Levelup." + className), new object[] { worldObj });

			return world;
		}

		/** Add elements to world. **/
		public void AddInnerWorld(World world) {
			InnerWorldsMap.Add(world._id, world);
		}

		public void AddMission(Mission mission) {
			Missions.Add(mission);
		}

		public void AddScore(Score score) {
			Scores.Add(score.ID, score);
		}

		/** Get elements to world. **/
		public World GetInnerWorldAt(int index) {
			if (index >= InnerWorldsMap.Count) {
				return null;
			}

			return InnerWorldsMap.Values.ElementAt(index);
		}

		/** Automatic generation of levels. **/
		private string IdForAutoGeneratedLevel(string id, int idx) {
			return id + "_level" + idx;
		}
		private string IdForAutoGeneratedScore(string id, int idx) {
			return id + "_score" + idx;
		}
		private string IdForAutoGeneratedGate(string id) {
			return id + "_gate";
		}
		private string IdForAutoGeneratedMission(string id, int idx) {
			return id + "_mission" + idx;
		}

		public void BatchAddLevelsWithTemplates(int numLevels, Gate gateTemplate, Score scoreTemplate, Mission missionTemplate) {
			List<Score> scoreTemplates = new List<Score>();
			if (scoreTemplate != null) {
				scoreTemplates.Add(scoreTemplate);
			}
			List<Mission> missionTemplates = new List<Mission>();
			if (missionTemplate != null) {
				missionTemplates.Add(missionTemplate);
			}

			BatchAddLevelsWithTemplates(numLevels, gateTemplate, scoreTemplates, missionTemplates);
		}

		public void BatchAddLevelsWithTemplates(int numLevels, Gate gateTemplate, List<Score> scoreTemplates, List<Mission>missionTemplates) {
			for (int i=0; i<numLevels; i++) {
				string lvlId = IdForAutoGeneratedLevel(_id, i);
				Level aLvl = new Level(lvlId);

				aLvl.Gate = gateTemplate.Clone(IdForAutoGeneratedGate(lvlId));

				if (scoreTemplates != null) {
					for(int k=0; k<scoreTemplates.Count(); k++) {
						aLvl.AddScore(scoreTemplates[k].Clone(IdForAutoGeneratedScore(lvlId, k)));
					}
				}

				if (missionTemplates != null) {
					for(int k=0; k<missionTemplates.Count(); k++) {
						aLvl.AddMission(missionTemplates[k].Clone(IdForAutoGeneratedMission(lvlId, k)));
					}
				}
				
				this.InnerWorldsMap.Add(lvlId, aLvl);
			}
		}


		/** For Single Score **/

		public void SetSingleScoreValue(double amount) {
			if (Scores.Count() == 0) {
				return;
			}
			SetScoreValue(Scores.First().Value.ID, amount);
		}

		public void DecSingleScore(double amount) {
			if (Scores.Count() == 0) {
				return;
			}
			DecScore(Scores.First().Value.ID, amount);
		}

		public void IncSingleScore(double amount) {
			if (Scores.Count() == 0) {
				return;
			}
			IncScore(Scores.First().Value.ID, amount);
		}

		public Score GetSingleScore() {
			if (Scores.Count() == 0) {
				return null;
			}
			return Scores.First().Value;
		}

		public double SumInnerWorldsRecords() {
			double ret = 0;
			foreach(World world in InnerWorldsList) {
				ret += world.GetSingleScore().Record;
			}
			return ret;
		}




		/** For more than one Score **/

		public void ResetScores(bool save) {
			if (Scores == null || Scores.Count == 0) {
				SoomlaUtils.LogError(TAG, "(ResetScores) You don't have any scores defined in this world. World id: " + _id);
				return;
			}

			foreach (Score score in Scores.Values) {
				score.Reset(save);
			}
		}

		public void DecScore(string scoreId, double amount) {
			Scores[scoreId].Dec(amount);
		}

		public void IncScore(string scoreId, double amount) {
			Scores[scoreId].Inc(amount);
		}

		public Dictionary<string, double> GetRecordScores() {
			Dictionary<string, double> records = new Dictionary<string, double>();
			foreach(Score score in Scores.Values) {
				records.Add(score.ID, score.Record);
			}

			return records;
		}

		public Dictionary<string, double> GetLatestScores() {
			Dictionary<string, double> latest = new Dictionary<string, double>();
			foreach (Score score in Scores.Values) {
				latest.Add(score.ID, score.Latest);
			}

			return latest;
		}

		public void SetScoreValue(string id, double scoreVal) {
			SetScoreValue(id, scoreVal, false);
		}

		public void SetScoreValue(string id, double scoreVal, bool onlyIfBetter) {
			Score score = Scores[id];
			if (score == null) {
				SoomlaUtils.LogError(TAG, "(setScore) Can't find score id: " + id + "  world id: " + this._id);
				return;
			}
			score.SetTempScore(scoreVal, onlyIfBetter);
		}


		/** Completion **/

		public bool IsCompleted() {
			return WorldStorage.IsCompleted(this);
		}

		public virtual void SetCompleted(bool completed) {
			SetCompleted(completed, false);
		}
		public void SetCompleted(bool completed, bool recursive) {
			if (recursive) {
				foreach (World world in InnerWorldsMap.Values) {
					world.SetCompleted(completed, true);
				}
			}
			WorldStorage.SetCompleted(this, completed);
		}


		/** Reward Association **/

		public void AssignReward(Reward reward) {
			String olderReward = GetAssignedRewardId();
			if (!string.IsNullOrEmpty(olderReward)) {
				Reward oldReward = LevelUp.GetInstance().GetReward(olderReward);
				if (oldReward != null) {
					oldReward.Take();
				}
			}

			// We have to make sure the assigned reward can be assigned unlimited times.
			// There's no real reason why it won't be.
			if (reward.Schedule.ActivationLimit > 0) {
				reward.Schedule.ActivationLimit = 0;
			}

			reward.Give();
			WorldStorage.SetReward(this, reward.ID);
		}

		public String GetAssignedRewardId() {
			return WorldStorage.GetAssignedReward(this);
		}

		public bool CanStart() {
			return Gate == null || Gate.IsOpen();
		}

	}
}

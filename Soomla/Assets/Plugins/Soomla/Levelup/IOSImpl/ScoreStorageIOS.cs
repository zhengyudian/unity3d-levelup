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

namespace Soomla.Levelup
{
#if UNITY_IOS && !UNITY_EDITOR

	[DllImport ("__Internal")]
	private static extern void scoreStorage_SetLatestScore(IntPtr scoreJson, double latest);
	[DllImport ("__Internal")]
	private static extern int scoreStorage_GetLatestScore(IntPtr scoreJson);
	[DllImport ("__Internal")]
	private static extern void scoreStorage_SetRecordScore(IntPtr scoreJson, double record);
	[DllImport ("__Internal")]
	private static extern int scoreStorage_GetRecordScore(IntPtr scoreJson);


	override protected void _setLatestScore(Score score, double latest) {
		string scoreJson = score.toJSONString();
		scoreStorage_SetLatestScore(scoreJson, latest);
	}
	
	override protected double _getLatestScore(Score score) {
		string scoreJson = score.toJSONString();
		return scoreStorage_GetLatestScore(scoreJson);
	}
	
	override protected void _setRecordScore(Score score, double record) {
		string scoreJson = score.toJSONString();
		scoreStorage_SetRecordScore(scoreJson, record);
	}
	
	override protected double _getRecordScore(Score score) {
		string scoreJson = score.toJSONString();
		return scoreStorage_GetRecordScore(scoreJson);
	}

#endif
}

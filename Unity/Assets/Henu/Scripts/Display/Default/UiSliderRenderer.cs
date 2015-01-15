﻿using System;
using Henu.Navigation;
using Henu.Settings;
using Henu.State;
using UnityEngine;

namespace Henu.Display.Default {

	/*================================================================================================*/
	public class UiSliderRenderer : MonoBehaviour, IUiArcSegmentRenderer {

		protected ArcState vArcState;
		protected ArcSegmentState vSegState;
		protected float vAngle0;
		protected float vAngle1;
		protected ArcSegmentSettings vSettings;
		protected NavItemSlider vNavSlider;
		protected int vMeshSteps;

		protected float vSliderAngleHalf;
		protected float vSlideDegree0;
		protected float vSlideDegrees;

		protected float vMainAlpha;
		protected float vAnimAlpha;

		protected GameObject vTrack;
		protected GameObject vFill;
		protected GameObject vGrabHold;
		protected Mesh vTrackMesh;
		protected Mesh vFillMesh;
		protected UiSliderGrabRenderer vGrab;


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public virtual void Build(ArcState pArcState, ArcSegmentState pSegState,
														float pArcAngle, ArcSegmentSettings pSettings) {
			vArcState = pArcState;
			vSegState = pSegState;
			vAngle0 = -pArcAngle/2f+UiSelectRenderer.AngleInset;
			vAngle1 = pArcAngle/2f-UiSelectRenderer.AngleInset;
			vSettings = pSettings;
			vNavSlider = (NavItemSlider)vSegState.NavItem;
			vMeshSteps = (int)Math.Round(Math.Max(2, (vAngle1-vAngle0)/Math.PI*60));

			const float pi = (float)Math.PI;

			vSliderAngleHalf = pi/80f;
			vSlideDegree0 = (vAngle0+vSliderAngleHalf)/pi*180;
			vSlideDegrees = (vAngle1-vAngle0-vSliderAngleHalf*2)/pi*180;

			////

			vTrack = new GameObject("Track");
			vTrack.transform.SetParent(gameObject.transform, false);
			vTrack.AddComponent<MeshFilter>();
			vTrack.AddComponent<MeshRenderer>();
			vTrack.renderer.sharedMaterial = new Material(Shader.Find("Unlit/AlphaSelfIllum"));
			vTrack.renderer.sharedMaterial.renderQueue -= 200;
			vTrack.renderer.sharedMaterial.color = Color.clear;

			vFill = new GameObject("Fill");
			vFill.transform.SetParent(gameObject.transform, false);
			vFill.AddComponent<MeshFilter>();
			vFill.AddComponent<MeshRenderer>();
			vFill.renderer.sharedMaterial = new Material(Shader.Find("Unlit/AlphaSelfIllum"));
			vFill.renderer.sharedMaterial.renderQueue -= 100;
			vFill.renderer.sharedMaterial.color = Color.clear;

			vTrackMesh = vTrack.GetComponent<MeshFilter>().mesh;
			vFillMesh = vFill.GetComponent<MeshFilter>().mesh;

			////

			vGrabHold = new GameObject("GrabHold");
			vGrabHold.transform.SetParent(gameObject.transform, false);

			var grabObj = new GameObject("Grab");
			grabObj.transform.SetParent(vGrabHold.transform, false);

			vGrab = grabObj.AddComponent<UiSliderGrabRenderer>();
			vGrab.Build(vArcState, vSegState, vSliderAngleHalf*2, pSettings);
		}

		/*--------------------------------------------------------------------------------------------*/
		public virtual void Update() {
			vMainAlpha = GetArcAlpha(vArcState)*vAnimAlpha;

			if ( !vSegState.NavItem.IsEnabled ) {
				vMainAlpha *= 0.333f;
			}

			float currVal = vNavSlider.CurrentValue;
			float showVal = currVal;

			if ( vArcState.IsLeft ) {
				BuildMesh(vTrackMesh, showVal, 1, false);
				BuildMesh(vFillMesh, 0, showVal, true);
			}
			else {
				showVal = 1-currVal;
				BuildMesh(vTrackMesh, 0, showVal, true);
				BuildMesh(vFillMesh, showVal, 1, false);
			}

			Color colTrack = vSettings.SliderTrackColor;
			Color colFill = vSettings.SliderFillColor;

			colTrack.a *= vMainAlpha;
			colFill.a *= vMainAlpha;

			vTrack.renderer.sharedMaterial.color = colTrack;
			vFill.renderer.sharedMaterial.color = colFill;

			float slideDeg = vSlideDegree0 + vSlideDegrees*showVal;
			vGrabHold.transform.localRotation = Quaternion.AngleAxis(slideDeg, Vector3.up);
		}

		/*--------------------------------------------------------------------------------------------*/
		public virtual void HandleChangeAnimation(bool pFadeIn, int pDirection, float pProgress) {
			float a = 1-(float)Math.Pow(1-pProgress, 3);
			vAnimAlpha = (pFadeIn ? a : 1-a);
			vGrab.HandleChangeAnimation(pFadeIn, pDirection, pProgress);
		}

		/*--------------------------------------------------------------------------------------------*/
		public float CalculateCursorDistance(Vector3 pCursorWorldPosition) {
			return vGrab.CalculateCursorDistance(pCursorWorldPosition);
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void BuildMesh(Mesh pMesh, float pAmount0, float pAmount1, bool pIsFill) {
			float sliderAngle = (vSliderAngleHalf+UiSelectRenderer.AngleInset)*2;
			float angleRange = vAngle1-vAngle0-sliderAngle;
			int fillSteps = (int)Math.Round((vMeshSteps-2)*(pAmount1-pAmount0))+2;
			float a0 = vAngle0 + angleRange*pAmount0;
			float a1 = vAngle0 + angleRange*pAmount1;

			if ( !pIsFill ) {
				a0 += sliderAngle;
				a1 += sliderAngle;
			}

			MeshUtil.BuildRingMesh(pMesh, 1.04f, 1.46f, a0, a1, fillSteps);
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public static float GetArcAlpha(ArcState pArcState) {
			float alpha = 1-(float)Math.Pow(1-pArcState.Strength, 2);
			alpha -= (float)Math.Pow(pArcState.GrabStrength, 2);
			return Math.Max(0, alpha);
		}

	}

}

﻿/**
 * Copyright (c) 2017 The Campfire Union Inc - All Rights Reserved.
 *
 * Licensed under the MIT license. See LICENSE file in the project root for
 * full license information.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Collections;

namespace VRKeys {

	/// <summary>
	/// Example use of VRKeys keyboard.
	/// </summary>
	public class DemoScene : MonoBehaviour {

		/// <summary>
		/// Reference to the VRKeys keyboard.
		/// </summary>
		public Keyboard keyboard;

		/// <summary>
		/// See the following for why this is so convoluted:
		/// http://referencesource.microsoft.com/#System.ComponentModel.DataAnnotations/DataAnnotations/EmailAddressAttribute.cs,54
		/// http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx/
		/// </summary>
		private Regex emailValidator = new Regex (@"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$", RegexOptions.IgnoreCase);

		/// <summary>
		/// Show the keyboard with a custom input message. Attaching events dynamically,
		/// but you can also use the inspector.
		/// </summary>
		private void OnEnable () {
			// Automatically creating camera here to show how
			GameObject camera = new GameObject ("Main Camera");
			Camera cam = camera.AddComponent<Camera> ();
			cam.nearClipPlane = 0.1f;
			camera.AddComponent<AudioListener> ();

			// Improves event system performance
			Canvas canvas = keyboard.canvas.GetComponent<Canvas> ();
			canvas.worldCamera = cam;

			keyboard.Enable ();
			keyboard.SetPlaceholderMessage ("Please enter your email address");
            
			keyboard.OnSubmit.AddListener (HandleSubmit);
			keyboard.OnCancel.AddListener (HandleCancel);
		}

		private void OnDisable () {
			keyboard.OnSubmit.RemoveListener (HandleSubmit);
			keyboard.OnCancel.RemoveListener (HandleCancel);

			keyboard.Disable ();
		}

		/// <summary>
		/// Validate the email and simulate a form submission. Connect this to OnSubmit.
		/// </summary>
		public void HandleSubmit (string text) {
			keyboard.DisableInput ();
		}

		public void HandleCancel () {
			Debug.Log ("Cancelled keyboard input!");
		}
	}
}
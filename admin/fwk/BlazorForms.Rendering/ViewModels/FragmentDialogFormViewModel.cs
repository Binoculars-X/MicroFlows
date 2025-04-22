using BlazorForms.Platform;
using BlazorForms.Rendering.Interfaces;
using BlazorForms.Rendering.Validation;
using BlazorForms.Shared.FastReflection;
using BlazorForms.Shared.Reflection;
using BlazorForms.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorForms.Flows.Definitions;
using BlazorForms.FlowRules;

namespace BlazorForms.Rendering.ViewModels;

public class FragmentDialogFormViewModel : FragmentViewModel, IFragmentDialogFormViewModel
{
    public bool DialogIsOpen { get; set; }
    public string? ItemId { get; private set; }
    public string? FlowType { get; private set; }
    public string LastAction { get; set; }

    public FragmentDialogFormViewModel(ILogger<FragmentViewModel> logger, IFlowRunProvider flowRunProvider, IDynamicFieldValidator dynamicFieldValidator,
       IJsonPathNavigator jsonPathNavigator, IModelNavigator modelNavi, IReflectionProvider reflectionProvider, NavigationManager navigationManager,
       IModelBindingNavigator modelBindingNavigator)
            : base(logger, flowRunProvider, dynamicFieldValidator, jsonPathNavigator, modelNavi, reflectionProvider, navigationManager,
                  modelBindingNavigator)
    {
    }

    public async Task LoadDialogForm(string formName, IFlowModel model, string? pk = null, FlowParamsGeneric? fps = null)
    {
        if (string.IsNullOrEmpty(formName))
        {
            throw new Exception("Form name must be supplied");
        }

        if (model == null)
        {
            throw new Exception("Model must be supplied");
        }

        var flowParams = fps ?? new FlowParamsGeneric();
        flowParams.ItemId = pk;
        flowParams["BaseUri"] = _navigationManager.BaseUri;
        Params = flowParams;

        ModelUntyped = model;
        FormId = formName;

        await ReloadFormData();
        DialogIsOpen = true;
    }

    public async Task LoadDialog(string flowName, FlowParamsGeneric parameters)
    {
        throw new NotImplementedException();
    }

    public async Task ValidateDialog()
    {
        var result = await TriggerRules(FormData.ProcessTaskTypeFullName, null, FormRuleTriggers.Submit);
        Validations = result.Validations.AsEnumerable().Union(GetDynamicFieldValidations());
    }

    public async Task SubmitDialog()
    {
        await ValidateDialog();

        if (Validations.Any(v => v.ValidationResult == RuleValidationResult.Error))
        {
            return;
        }

        //if (FormSettings.AllowFlowStorage)
        //{
        //    Context = await _flowRunProvider.SubmitListItemForm(RefId, ModelUntyped, SubmitActionName);
        //}
        //else
        //{
        //    var ctx = await _flowRunProvider.SubmitClientKeptContextFlowForm(Context.GetClientContext(), ModelUntyped,
        //        Params as FlowParamsGeneric, SubmitActionName);

        //    Context = ctx.GetClientContext();
        //}

        //PopulateException(Context);
        ClearData();
        DialogIsOpen = false;
    }
}

﻿using Raml.Tools.Tests;
using System;
using System.Threading.Tasks;

namespace AMF.Tools.TestRunner
{
    class Program
    {
        private static int TestCount = 0;

        static int Main(string[] args)
        {
            try
            {
                TestCount = 0;
                RunExchangeTestsAsync().Wait();
                RunOasTestsAsync().Wait();
                RunServerRaml1TestsAsync().Wait();
                RunWebApiTestsAsync().Wait();
                RunClientRaml1TestsAsync().Wait();
                Console.WriteLine($"{TestCount} tests passed");
                return 0;
            }
            catch (Exception ex)
            {
                InformException(ex);
                return 1;
            }
        }

        private static async Task RunExchangeTestsAsync()
        {
            var tests = new RamlExchangeTests();

            await tests.zendesk_api_raml(); // RP-578
            await tests.twitter_api_raml(); // RP 577
            await tests.stripe_api_raml(); // RP 576
            await tests.salesforce_raml_raml(); // RP-575
            await tests.google_drive_api_raml(); // RP-574
            await tests.github_api_raml(); // RP-573
            await tests.box_api_raml(); // RP-572
            await tests.catalyst_healthcare_appointment_api_raml();
            await tests.banking_aisp_experience_api();
            await tests.appian_api_raml();
            await tests.catalyst_retail_partners_api_raml();
            await tests.devrel_quick_start_product_api_raml();
            await tests.here_geocoder_api_autocomplete_raml();
            await tests.here_geocoder_api_batch_raml();
            await tests.here_geocoder_api_forward_raml();
            await tests.here_geocoder_api_reverse_raml();
            await tests.paypal_payments_api_oas();
            await tests.paypal_payments_api_raml();
            await tests.quick_start_nto_orders_api_raml();
            await tests.training_american_flights_api_oas();
            await tests.training_american_flights_api_raml();
            await tests.account_aggregation_process_api_raml();
            await tests.anypoint_bank_experience_api_raml();
            await tests.authorization_server_raml();
            await tests.catalyst_banking_as400_system_api_raml();
            await tests.catalyst_banking_payment_api_raml();
            await tests.catalyst_retail_customer_onboarding_raml();
            await tests.catalyst_retail_customer_system_api_raml();
            await tests.catalyst_retail_fulfilment_process_raml();
            await tests.catalyst_retail_omnichannel_xp_api_raml();
            await tests.catalyst_retail_order_system_api_raml();
            await tests.catalyst_retail_partner_system_api_raml();
            await tests.catalyst_retail_payment_process_raml();
            await tests.catalyst_retail_shopping_cart_proc_raml();
            await tests.customer_system_api_raml();
            await tests.payment_initiation_api_raml();
            await tests.qoppa_api_raml();
            await tests.anypoint_clinic_experience_api_raml();
            await tests.api_designer_experience_api_raml();
            await tests.appointments_process_api_raml();
            await tests.atm_and_branch_location_api_raml();
            await tests.cardconnect_rest_api_raml();
            await tests.catalyst_healthcare_ehr_to_crm_papi_raml();
            await tests.catalyst_healthcare_fitness_papi_raml();
            await tests.catalyst_healthcare_portal_api_raml();
            await tests.catalyst_retail_inventory_api_raml();
            await tests.catalyst_retail_location_api_raml();
            await tests.catalyst_retail_notification_api_raml();
            await tests.catalyst_retail_product_api_raml();
            await tests.catalyst_retail_product_availabilit_raml();
            await tests.ehr_fhir_system_api_raml();
            await tests.fhir_apis_raml();
            await tests.fitness_fhir_system_api_raml();
            await tests.google_contacts_api_raml();
            await tests.linkedin_api_raml();
            await tests.mule_twilio_connector_raml();
            await tests.new_relic_api_raml();
            await tests.nexmo_messages_api_raml();
            await tests.nexmo_sms_api_raml();
            await tests.open_bank_system_api_raml();
            await tests.optymyze_api_raml();
            await tests.pega_api_raml();
            await tests.pokitdok_pharmacy_coverage_api_raml();
            await tests.stibo_api_raml();
            await tests.tutorial_cookbook_raml_raml();
            await tests.workiva_wdesk_spreadsheets_api_raml();
            await tests.yammer_raml_raml();
            await tests.zuora_raml_raml();

            //
            await tests.paypal_orders_api_raml();
            await tests.catalyst_healthcare_fitbit_sapi_raml();
            await tests.catalyst_healthcare_onboarding_api_raml();
            await tests.crm_fhir_system_api_raml();
            await tests.customer_api_for_visual_editing_raml();

            TestCount += tests.TestCount;
        }

        private static async Task RunOasTestsAsync()
        {
            var tests = new OasTests();
            await tests.PetStoreClient();
            await tests.PetStoreServer();
            TestCount += tests.TestCount;
        }

        private static async Task RunClientRaml1TestsAsync()
        {
            var tests = new ClientGeneratorRaml1Tests();
            await tests.ShouldHandleEnums();
            await tests.ShouldBuildTypes_WhenMovies();
            await tests.ShouldBuildUriParameter_WhenCustomScalar();
            await tests.ShouldBuild_WhenCustomScalar();
            await tests.ShouldBuild_WhenMovieType();
            await tests.ShouldBuild_WhenParameters();
            await tests.ShouldHandleArrayAsExpression();
            await tests.ShouldHandleArrayItemAsScalar();
            await tests.ShouldHandleInlinedTypes();
            await tests.ShouldHandleShortcutsSyntacticSugar();

            // await tests.ShouldHandleTypeExpressions();

            // TODO: collections defined in types
            //await tests.ShouldBuildArrayTypes();
            TestCount += tests.TestCount;
        }

        private static async Task RunWebApiTestsAsync()
        {
            var tests = new WebApiGeneratorTests();
            await tests.ShouldWorkIncludeWithRelativeIncludes();
            // await tests.ShouldWorkIncludeWithIncludes();
            TestCount += tests.TestCount;
        }

        private static async Task RunServerRaml1TestsAsync()
        {
            var tests = new WebApiGeneratorRaml1Tests();
            await tests.OGame_Test();

            await tests.ShouldHandeInheritance();
            await tests.ShouldMapPatternAttributes();
            await tests.ShouldBuildTypes_WhenMovies();
            await tests.ShouldDetectArrayTypes_WhenMovies();
            await tests.ShouldBuild_WhenMovieType();
            await tests.ShouldBuild_WhenChinook();
            await tests.ShouldBuildArrays();
            await tests.ShouldBuildDependentTypes();
            await tests.ShouldBuild_EvenWithDisorderedTypes();
            await tests.ShouldBuild_WhenCustomScalar();
            await tests.ShouldBuild_WhenParameters();
            await tests.ShouldBuild_WhenTypeExpressions();
            await tests.ShouldDiffientiateBetweenTypesAndBaseTypes();
            await tests.ShouldHandleAnyType();
            await tests.ShouldApplyParametersOfResourceType();
            await tests.ShouldHandleEnumsAtRootLevel();
            await tests.ShouldHandleSameNameEnclosingType();
            await tests.ShouldHandleCasing();
            await tests.ShouldHandleDates();
            await tests.ShouldHandleNullDescription();
            await tests.ShouldHandleNumberFormats();
            await tests.ShouldHandleNumberFormatsOnRaml08_v3Schema();
            await tests.ShouldHandleNumberFormatsOnRaml08_v4Schema();
            await tests.ShouldHandleRouteNameContainedInUriParam();
            await tests.ShouldHandleSimilarSchemas();
            await tests.ShouldHandle_FileTypes();

            // TODO: https://www.mulesoft.org/jira/browse/APIMF-927
            await tests.ShouldHandleTraitsInLibraries();
            // TODO: https://www.mulesoft.org/jira/browse/APIMF-927
            await tests.ShouldHandle_SalesOrdersCase();

            // TODO: https://www.mulesoft.org/jira/browse/APIMF-891
            await tests.ShouldHandleXml();

            //TODO: 
            //await tests.ShouldHandleUnionTypes();

            //TODO: check
            //await tests.ShouldHandleComplexQueryParams();
            TestCount += tests.TestCount;
        }

        private static void InformException(Exception ex)
        {
            if (ex.InnerException?.GetType().Name == "AssertionException")
            {
                Console.WriteLine(ex.InnerException.Message);
                Console.WriteLine(ex.InnerException.StackTrace);
                return;
            }

            if (ex.InnerException != null)
            {
                Console.WriteLine(ex.InnerException.Message);
                if (string.IsNullOrWhiteSpace(ex.InnerException.StackTrace))
                    Console.WriteLine(ex.StackTrace);
                else
                    Console.WriteLine(ex.InnerException.StackTrace);
                return;
            }

            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }
}

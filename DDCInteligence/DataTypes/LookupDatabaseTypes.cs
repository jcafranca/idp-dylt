using DDCInteligence.Utilities.Helpers;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DDCInteligence.DataTypes
{
    public class LookupDatabaseTypes
    {

        [Table("client_tbl")]
        public class ClientInfo
        {
            public string clientId { get; set; }
            public string name { get; set; }
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string city { get; set; }
            public string provine { get; set; }
            public string postalcode { get; set; }
            public string additiondate { get; set; }
            public string isinactive { get; set; }
            public string creditstatus { get; set; }
            public string billcustomer { get; set; }
            public string instimestamp { get; set; }
        }

        [Table("client_commodity_tbl")]
        public class ClientCommodityInfo
        {
            public string clientid { get; set; }
            public string commoditycode { get; set; }
            public string altcommoditydesc { get; set; }
        }

        [Table("customer_tbl")]
        public class CustomerInfo
        {
            public string ACCOUNT_CODE { get; set; }
            public string TERMINAL { get; set; }
            public string NAME { get; set; }
            public string ADDR_1 { get; set; }
            public string ADDR_2 { get; set; }
            public string ADDR_3 { get; set; }
            public string CITY { get; set; }
            public string STATE { get; set; }
            public string ZIP { get; set; }
            public string PARENT_PROFIT_NUM { get; set; }
        }
        [Table("billto_tbl")]
        public class BillToInfo
        {
            public string ACCOUNT_CODE { get; set; }
            public string P_C_CODE { get; set; }
            public string NAME { get; set; }
            public string ADD1 { get; set; }
            public string ADD2 { get; set; }
            public string ADD3 { get; set; }
            public string CITY { get; set; }
            public string STATE { get; set; }
            public string ZIP { get; set; }
            public string UPD_DATE { get; set; }
            public string UPD_TIME { get; set; }
            public string TERM_ID { get; set; }
            public string USER_ID { get; set; }
            public string DUMP_DATE { get; set; }
            public string DPBC { get; set; }
            public string USPS_CERT { get; set; }
            public string BILL_TO_KEY { get; set; }
        }
        
        [Table("points_tbl")]
        public class PointInfo
        {
            public string STATE { get; set; }
            public string POINT { get; set; }
            public string ZIP_CODE { get; set; }
            public string TERM { get; set; }
        }

        [Table("aliases_tbl")]
        public class AliasInfo 
        {
            public string ACCOUNT_CODE { get; set; }
            public string ALIAS_NAME { get; set; }
            public string ADDR_1 { get; set; }
            public string CITY { get; set; }
            public string STATE { get; set; }
            public string ZIP { get; set; }
            public string ALIAS_DATE { get; set; }
            public string ALIAS_TIME { get; set; }
            public string FLAGGED_ALIAS_FLAG { get; set; }
        }

        [Table("third_party_tbl")]
        public class ThirdPartyInfo
        {
            public string ACCOUNT_CODE { get; set; }
            public string TP_TERMINAL { get; set; }
            public string TP_NAME { get; set; }
            public string ADDR_1 { get; set; }
            public string ADDR_2 { get; set; }
            public string ADDR_3 { get; set; }
            public string CITY { get; set; }
            public string STATE { get; set; }
            public string ZIP { get; set; }
            public string PARENT_PROFIT_NUM { get; set; }
        }

        [Table("aliases_tp_tbl")]
        public class AliasTpInfo
        {
            public string ACCOUNT_CODE { get; set; }
            public string ALIAS_TP_NAME { get; set; }
            public string ADDR_1 { get; set; }
            public string ADDR_2 { get; set; }
            public string CITY { get; set; }
            public string STATE { get; set; }
            public string ZIP { get; set; }
            public string UPD_DATE { get; set; }
            public string UPD_TIME { get; set; }
            public string FLAGGED { get; set; }
        }

        [Table("b_l_imag_dtl_tbl")]
        public class DTLInfo 
        {
            public string CUST { get; set; }
            public string EFF_DATE { get; set; }
            public string LINE_NO { get; set; }
            public string LINE_SUB { get; set; }
            public string DESCR { get; set; }
        }

        [Table("b_l_imag_hdr_tbl")]
        public class HDRInfo 
        {
            public string CUST { get; set; }
            public string EFF_DATE { get; set; }
            public string LINE_NO { get; set; }
            public string REF_NUM { get; set; }
            public string LINE_SUB { get; set; }
            public string DESCR { get; set; }
            public string HM { get; set; }
            public string HM_CLS { get; set; }
            public string HM_UN { get; set; }
            public string HM_PG { get; set; }
            public string CLASS { get; set; }
            public string DETAILS { get; set; }
            public string NMFC_NUM { get; set; }
            public string NMFC_SUB { get; set; }
        }

        [Table("spec_hd_codes_tbl")]
        public class SpecHDInfo
        {
            public string CODE { get; set; }
            public string DESCR { get; set; }
            public string PRIORITY { get; set; }
            public string SPEC_HD_CODE_ORDER { get; set; }
        }

        [Table("cust_prompt_tbl")]
        public class CustomerPrompt
        {
            public string ACCT_CODE { get; set; }
            public string SHIPQ_BC_1 { get; set; }
            public string SHIPQ_BC_2 { get; set; }
            public string SHIPQ_BC_3 { get; set; }
            public string SHIPQ_BC_4 { get; set; }
            public string SHIPQ_BC_5 { get; set; }
            public string SHIPQ_BC_6 { get; set; }
            public string SHIPQ_BC_7 { get; set; }
            public string SHIPQ_BC_8 { get; set; }
            public string SHIPQ_BC_9 { get; set; }
            public string SHIPQ_BC_10 { get; set; }
            public string CONS_DPO_FLAG { get; set; }
            public string SHIP_DPO_FLAG { get; set; }
            public string PC { get; set; }
            public string PC_PROMPT_1 { get; set; }
            public string PC_PROMPT_2 { get; set; }
            public string WHEN_SHIP_1 { get; set; }
            public string WHEN_SHIP_2 { get; set; }
            public string WHEN_CONS_1 { get; set; }
            public string WHEN_CONS_2 { get; set; }
            public string WHEN_TP_1 { get; set; }
            public string WHEN_TP_2 { get; set; }
            public string FI_ACCT_1 { get; set; }
            public string FI_CODE_1 { get; set; }
            public string FI_PROMPT_1 { get; set; }
            public string FI_VALUE_1 { get; set; }
            public string FI_ACCT_2 { get; set; }
            public string FI_CODE_2 { get; set; }
            public string FI_PROMPT_2 { get; set; }
            public string FI_VALUE_2 { get; set; }
            public string FI_ACCT_3 { get; set; }
            public string FI_CODE_3 { get; set; }
            public string FI_PROMPT_3 { get; set; }
            public string FI_VALUE_3 { get; set; }
            public string FI_ACCT_4 { get; set; }
            public string FI_CODE_4 { get; set; }
            public string FI_PROMPT_4 { get; set; }
            public string FI_VALUE_4 { get; set; }
            public string FI_ACCT_5 { get; set; }
            public string FI_CODE_5 { get; set; }
            public string FI_PROMPT_5 { get; set; }
            public string FI_VALUE_5 { get; set; }
            public string FI_ACCT_6 { get; set; }
            public string FI_CODE_6 { get; set; }
            public string FI_PROMPT_6 { get; set; }
            public string FI_VALUE_6 { get; set; }
            public string PERMANET_F11_PRMPT { get; set; }
            public string SHP_PROMPT_1 { get; set; }
            public string SHP_PROMPT_2 { get; set; }
            public string CON_PROMPT_1 { get; set; }
            public string CON_PROMPT_2 { get; set; }
            public string SI_PROMPT_ACCT { get; set; }
            public string SI_PROMPT_1 { get; set; }
            public string SI_PROMPT_2 { get; set; }
            public string SI_ACCT_1 { get; set; }
            public string SI_AUTO_LOAD_1 { get; set; }
            public string SI_ACCT_2 { get; set; }
            public string SI_AUTO_LOAD_2 { get; set; }
            public string SI_ACCT_3 { get; set; }
            public string SI_AUTO_LOAD_3 { get; set; }
            public string SHC_PROMPT_ACCT { get; set; }
            public string SHC_PROMPT { get; set; }
            public string SHC_ACCT_1 { get; set; }
            public string SHC_AUTO_LOAD_1 { get; set; }
            public string SHC_ACCT_2 { get; set; }
            public string SHC_AUTO_LOAD_2 { get; set; }
            public string SHC_ACCT_3 { get; set; }
            public string SHC_AUTO_LOAD_3 { get; set; }
            public string COD_PROMPT_ACCT { get; set; }
            public string COD_PROMPT { get; set; }
            public string UPD_DATE { get; set; }
            public string UPD_TIME { get; set; }
            public string USER_ID { get; set; }
            public string CHG_PRM_DATE { get; set; }
            public string BILL_REGION_1 { get; set; }
            public string BILL_REGION_2 { get; set; }
            public string BILL_REGION_3 { get; set; }
            public string BILL_REGION_4 { get; set; }
            public string BILL_REGION_5 { get; set; }
        }

        [Table("type_charge_codes_tbl")]
        public class ChargesCodesInfo
        {
            public string CODE { get; set; }
            public string COMP_RESP { get; set; }
            public string TDCC_150 { get; set; }
        }
    }
}

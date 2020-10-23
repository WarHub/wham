<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" 
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
                xmlns:bsr="http://www.battlescribe.net/schema/rosterSchema" 
                xmlns="http://www.battlescribe.net/schema/rosterSchema"
                exclude-result-prefixes="bsr">

    <xsl:output method="xml" indent="yes"/>

    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="/bsr:roster/@battleScribeVersion">
        <xsl:attribute name="battleScribeVersion">1.15</xsl:attribute>
    </xsl:template>

    <xsl:template match="bsr:catalogueLinks">
        <forces>
            <xsl:apply-templates />
        </forces>
    </xsl:template>

    <xsl:template match="bsr:catalogueLink">
        <force>
            <xsl:apply-templates select="@id
                                        | @catalogueId
                                        | @catalogueRevision
                                        | @catalogueName
                                        | @forceTypeId
                                        | @forceTypeName
                                        | bsr:categories
                                        | bsr:catalogueLinks"/>
        </force>
    </xsl:template>

    <xsl:template match="bsr:category">
        <category>
            <xsl:apply-templates select="@id
                                        | @categoryId
                                        | @name"/>

            <selections>
                <xsl:apply-templates select="/bsr:roster/bsr:selections/bsr:selection[@categoryId = current()/@categoryId 
                                                                                      and @catalogueLinkId = current()/../../@id]"/>
            </selections>
        </category>
    </xsl:template>

    <xsl:template match="bsr:selection">
        <selection>
            <xsl:apply-templates select="@id
                                        | @entryId
                                        | @entryGroupId
                                        | @name
                                        | @points
                                        | @number
                                        | @type
                                        | @customName
                                        | bsr:selections
                                        | bsr:rules
                                        | bsr:profiles
                                        | bsr:customDescription"/>
        </selection>
    </xsl:template>
    
    <xsl:template match="/bsr:roster/bsr:selections
                        | bsr:selection/@catalogueLinkId
                        | bsr:selection/@categoryId"/>

</xsl:stylesheet>
